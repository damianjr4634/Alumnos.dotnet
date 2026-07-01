using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia del volcado de CNA (Confirmar333/Cna) contra el SP legacy XXX_REGULARIZACION.
/// CNA no tiene SP de condición (se decide en el cliente por la nota final), así que solo se
/// verifica el <b>commit</b>: como CNA es CARRERA.TIPO='BAC', usa la rama BAC de
/// XXX_REGULARIZACION. Se corre el mismo caso REGULAR por los dos caminos (SP sobre
/// "$$$CURSADA" vs <c>ConfirmarFilasCnaAsync</c>), cada uno en su transacción revertida, y se
/// compara el efecto en CURSADA/CURSADA_HST/ANALITIC. La base no se muta.
/// </summary>
[Trait("Category", "Integration")]
public class RegularizacionCnaEquivalenciaTests
{
    private const int UsuarioPrueba = 9994;
    private static readonly DateTime FechaFinal = new(2024, 7, 1);

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    private sealed record CursadaRef
    {
        public string Carre { get; init; } = string.Empty;
        public string CodAlu { get; init; } = string.Empty;
        public string CodMat { get; init; } = string.Empty;
        public string CuaAnio { get; init; } = string.Empty;
    }

    private sealed record Efecto(
        bool CursadaExiste, int HstCount, string? HstCondicion, string? HstCondant,
        decimal? AnaNota, DateTime? AnaFecha, string? AnaCondicion, string? AnaMatriz);

    [Fact]
    public async Task Regular_VuelcaIgualQueElSp()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        var cursada = await connection.QueryFirstOrDefaultAsync<CursadaRef>("""
            SELECT FIRST 1 TRIM(C.CARRE) AS Carre, TRIM(C.COD_ALU) AS CodAlu, TRIM(C.COD_MAT) AS CodMat, TRIM(C.CUA_ANIO) AS CuaAnio
            FROM CURSADA C
            WHERE C.CARRE = 'CNA' AND COALESCE(TRIM(C.CUA_ANIO), '') <> ''
              AND EXISTS(SELECT 1 FROM ALUMNOS A WHERE A.COD_ALU = C.COD_ALU AND A.CARRE = C.CARRE)
              AND NOT EXISTS(SELECT 1 FROM ANALITIC A WHERE A.CARRE = C.CARRE AND A.COD_ALU = C.COD_ALU AND A.COD_MAT = C.COD_MAT)
            """)
            ?? throw new InvalidOperationException("Se necesita una cursada CNA sin analítico para la prueba.");

        var efectoSp = await EfectoPorCaminoAsync(connection, cursada, async (conn, tx) =>
        {
            await PoblarStagingRegularAsync(conn, tx, cursada);
            _ = await conn.QueryFirstOrDefaultAsync<(int?, string?)>(new CommandDefinition(
                "SELECT FERRCOD, FERRMSG FROM XXX_REGULARIZACION(@Carre, @U)",
                new { cursada.Carre, U = UsuarioPrueba }, tx));
        });

        var fila = new FilaRegularizacionCnaResuelta
        {
            CodigoAlumno = cursada.CodAlu,
            CodigoMateria = cursada.CodMat,
            CuatrimestreAnio = cursada.CuaAnio,
            NuevaCondicion = "REGULAR",
            NotaFinal = 8m,
            Fecha = FechaFinal,
        };
        var efectoCs = await EfectoPorCaminoAsync(connection, cursada, (conn, tx) =>
            RegularizacionRepository.ConfirmarFilasCnaAsync(conn, tx, cursada.Carre, UsuarioPrueba, [fila], CancellationToken.None));

        Assert.False(efectoSp.CursadaExiste);
        Assert.Equal(8m, efectoSp.AnaNota);
        Assert.Equal("REGULAR", efectoSp.AnaCondicion);
        Assert.Equal(efectoSp, efectoCs);
    }

    // Copia la cursada real al staging (pass-through = identidad) y fija nota final/fecha/condición.
    private static async Task PoblarStagingRegularAsync(FbConnection conn, FbTransaction tx, CursadaRef c)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM \"$$$CURSADA\" WHERE USUARIO = @U", new { U = UsuarioPrueba }, tx));
        await conn.ExecuteAsync(new CommandDefinition("""
            INSERT INTO "$$$CURSADA" (USUARIO, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, CONDICION, FINAL1, FECHA1,
                                      TP_EVA, TP_EVA2, RECUP, REGULAR, TOT_HORAS, INASIST, JUSTIF, APELLIDO, MATRIZ)
            SELECT @U, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, 'REGULAR', 8, @Fecha,
                   TP_EVA, TP_EVA2, RECUP, REGULAR, TOT_HORAS, INASIST, JUSTIF, APELLIDO, MATRIZ
            FROM CURSADA
            WHERE CARRE = @Carre AND COD_ALU = @A AND COD_MAT = @M AND CUA_ANIO = @Cua
            """,
            new { U = UsuarioPrueba, c.Carre, A = c.CodAlu, M = c.CodMat, Cua = c.CuaAnio, Fecha = FechaFinal }, tx));
    }

    private static async Task<Efecto> EfectoPorCaminoAsync(
        FbConnection connection, CursadaRef c, Func<FbConnection, FbTransaction, Task> camino)
    {
        await using var tx = (FbTransaction)await connection.BeginTransactionAsync(CancellationToken.None);
        try
        {
            await camino(connection, tx);
            var p = new { c.Carre, c.CodAlu, c.CodMat };

            var cursadaExiste = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                "SELECT COUNT(*) FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat", p, tx)) > 0;
            var hstCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                "SELECT COUNT(*) FROM CURSADA_HST WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat", p, tx));
            var hst = await connection.QueryFirstOrDefaultAsync(new CommandDefinition("""
                SELECT FIRST 1 TRIM(CONDICION) AS CONDICION, TRIM(CONDANT) AS CONDANT
                FROM CURSADA_HST WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat ORDER BY INDICE DESC
                """, p, tx));
            var ana = await connection.QueryFirstOrDefaultAsync(new CommandDefinition("""
                SELECT NOTA_MAT, FEC_FINAL, TRIM(CONDICION) AS CONDICION, TRIM(MATRIZ) AS MATRIZ
                FROM ANALITIC WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
                """, p, tx));

            return new Efecto(
                cursadaExiste, hstCount, (string?)hst?.CONDICION, (string?)hst?.CONDANT,
                (decimal?)ana?.NOTA_MAT, (DateTime?)ana?.FEC_FINAL, (string?)ana?.CONDICION, (string?)ana?.MATRIZ);
        }
        finally
        {
            await tx.RollbackAsync(CancellationToken.None);
        }
    }
}
