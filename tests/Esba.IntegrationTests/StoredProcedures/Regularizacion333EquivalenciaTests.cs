using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Academica;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia de la regularización de secundario (333/650) portada a C# contra los SP
/// legacy: la <b>condición</b> (<see cref="CalculoCondicionRegularizacion333"/> vs
/// XXX_REGULARIZACION_MAT_333) y el <b>commit</b> (Confirmar333 vs la rama 333/650 de
/// XXX_REGULARIZACION). Todo en transacciones que se revierten: la base no se muta.
/// </summary>
[Trait("Category", "Integration")]
public class Regularizacion333EquivalenciaTests
{
    private const int UsuarioPrueba = 9995;
    private const string CondicionOrigen = "CURSANDO";

    private static readonly DateTime FecEva2 = new(2024, 7, 1);
    private static readonly DateTime FechDic = new(2024, 12, 15);
    private static readonly DateTime FechMar = new(2025, 3, 10);

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    private sealed record CursadaRef
    {
        public string Carre { get; init; } = string.Empty;
        public string CodAlu { get; init; } = string.Empty;
        public string CodMat { get; init; } = string.Empty;
        public short Cutuco { get; init; }
        public string CuaAnio { get; init; } = string.Empty;
    }

    private sealed record Escenario(decimal? TpEva, decimal? TpEva2, decimal? NotaDic, decimal? NotaMar);

    private static async Task<CursadaRef> BuscarCursadaAsync(FbConnection connection) =>
        await connection.QueryFirstOrDefaultAsync<CursadaRef>("""
            SELECT FIRST 1 TRIM(C.CARRE) AS Carre, TRIM(C.COD_ALU) AS CodAlu, TRIM(C.COD_MAT) AS CodMat,
                   C.CUTUCO AS Cutuco, TRIM(C.CUA_ANIO) AS CuaAnio
            FROM CURSADA C
            WHERE C.CARRE IN ('333','650') AND COALESCE(TRIM(C.CUA_ANIO), '') <> ''
              AND EXISTS(SELECT 1 FROM ALUMNOS A WHERE A.COD_ALU = C.COD_ALU AND A.CARRE = C.CARRE)
              AND NOT EXISTS(SELECT 1 FROM ANALITIC A WHERE A.CARRE = C.CARRE AND A.COD_ALU = C.COD_ALU AND A.COD_MAT = C.COD_MAT)
            """)
        ?? throw new InvalidOperationException("Se necesita una cursada 333/650 sin analítico para la prueba.");

    [Fact]
    public async Task Condicion_DominioYSp_Coinciden_ParaVariasNotas()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var cursada = await BuscarCursadaAsync(connection);

        var escenarios = new[]
        {
            new Escenario(0m, 7m, null, null),    // 2° trimestre aprobado → REGULAR (nota 7, fecha del 2° trim)
            new Escenario(5m, 4m, 0m, 0m),        // 2° desaprobado, sin dic/mar → ENPROCESO
            new Escenario(5m, 4m, 8m, 0m),        // diciembre aprueba → REGULAR (nota 8, fecha dic)
            new Escenario(5m, 4m, 0m, 7m),        // marzo aprueba → REGULAR (nota 7, fecha mar)
            new Escenario(5m, 4m, 3m, 0m),        // diciembre aplaza → PREVIA
            new Escenario(5m, 0m, 0m, 0m),        // 2° sin cargar → mantiene condición de origen
            new Escenario(99m, 99m, 0m, 0m),      // ambos trimestres ausentes, sin dic/mar → ENPROCESO
        };

        await using var tx = (FbTransaction)await connection.BeginTransactionAsync(CancellationToken.None);
        try
        {
            foreach (var e in escenarios)
            {
                var (condSp, notaFinSp, notaFinFechaSp) = await CondicionPorSpAsync(connection, tx, cursada, e);

                var r = CalculoCondicionRegularizacion333.Resolver(
                    new NotasRegularizacion333(CondicionOrigen, e.TpEva, e.TpEva2, e.NotaDic, e.NotaMar,
                        FecEva2, FechDic, FechMar));

                Assert.Equal(condSp, r.Condicion);
                Assert.Equal(notaFinSp, r.NotaFinal);
                Assert.Equal(notaFinFechaSp, r.NotaFinalFecha);
            }
        }
        finally
        {
            await tx.RollbackAsync(CancellationToken.None);
        }
    }

    private static async Task<(string? Condicion, decimal NotaFin, DateTime? NotaFinFecha)> CondicionPorSpAsync(
        FbConnection connection, FbTransaction tx, CursadaRef c, Escenario e)
    {
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM \"$$$CURSADA\" WHERE USUARIO = @U", new { U = UsuarioPrueba }, tx));
        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO "$$$CURSADA" (USUARIO, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, CONDICION,
                                      TP_EVA, TP_EVA2, FEC_EVA2, NOTADIC, FECHDIC, NOTAMAR, FECHMAR)
            VALUES (@U, @A, @M, @Cut, @Cua, @Cond, @TpEva, @TpEva2, @FecEva2, @NotaDic, @FechDic, @NotaMar, @FechMar)
            """,
            new
            {
                U = UsuarioPrueba, A = c.CodAlu, M = c.CodMat, Cut = c.Cutuco, Cua = c.CuaAnio, Cond = CondicionOrigen,
                e.TpEva, e.TpEva2, FecEva2, e.NotaDic, FechDic, e.NotaMar, FechMar,
            }, tx));

        _ = await connection.QueryFirstOrDefaultAsync<(int? FErrCod, string? FErrMsg)>(new CommandDefinition(
            "SELECT FERRCOD, FERRMSG FROM XXX_REGULARIZACION_MAT_333(@Carre, @A, @M, @CondOrig, @U)",
            new { c.Carre, A = c.CodAlu, M = c.CodMat, CondOrig = CondicionOrigen, U = UsuarioPrueba }, tx));

        return await connection.QueryFirstAsync<(string? Condicion, decimal NotaFin, DateTime? NotaFinFecha)>(
            new CommandDefinition(
                "SELECT TRIM(CONDICION), COALESCE(NOTAFIN,0), NOTAFIN_FECHA FROM \"$$$CURSADA\" WHERE USUARIO = @U AND COD_MAT = @M",
                new { U = UsuarioPrueba, M = c.CodMat }, tx));
    }

    [Fact]
    public async Task Regular_VuelcaIgualQueElSp()
    {
        // 2° trimestre 8 → REGULAR, NOTAFIN=8, NOTAFIN_FECHA = fecha del 2° trimestre.
        var edit = new Escenario333(TpEva: 6m, TpEva2: 8m, TpEva3: 7m, NotaDic: 0m, NotaMar: 0m,
            TotHoras: 100, Inasist: 5, Justif: 0);

        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var cursada = await BuscarCursadaAsync(connection);

        var efectoSp = await EfectoPorCaminoAsync(connection, cursada, async (conn, tx) =>
        {
            await PoblarStagingRegularAsync(conn, tx, cursada, edit);
            _ = await conn.QueryFirstOrDefaultAsync<(int?, string?)>(new CommandDefinition(
                "SELECT FERRCOD, FERRMSG FROM XXX_REGULARIZACION(@Carre, @U)",
                new { cursada.Carre, U = UsuarioPrueba }, tx));
        });

        var fila = new FilaRegularizacion333Resuelta
        {
            CodigoAlumno = cursada.CodAlu,
            CodigoMateria = cursada.CodMat,
            CuatrimestreAnio = cursada.CuaAnio,
            TpEva = edit.TpEva,
            TpEva2 = edit.TpEva2,
            TpEva3 = edit.TpEva3,
            FecEva1 = null,
            FecEva2 = FecEva2,
            FecEva3 = null,
            NotaDic = edit.NotaDic,
            FechDic = null,
            NotaMar = edit.NotaMar,
            FechMar = null,
            TotalHoras = (short)edit.TotHoras,
            Inasistencias = (short)edit.Inasist,
            Justificadas = (short)edit.Justif,
            Fecha = FecEva2,
            NuevaCondicion = "REGULAR",
            NotaFinal = 8m,
            NotaFinalFecha = FecEva2,
        };

        var efectoCs = await EfectoPorCaminoAsync(connection, cursada, (conn, tx) =>
            RegularizacionRepository.ConfirmarFilas333Async(conn, tx, cursada.Carre, UsuarioPrueba, [fila], CancellationToken.None));

        Assert.False(efectoSp.CursadaExiste);
        Assert.Equal(8m, efectoSp.AnaNota);
        Assert.Equal("REGULAR", efectoSp.AnaCondicion);
        Assert.Equal(efectoSp, efectoCs);
    }

    private sealed record Escenario333(
        decimal TpEva, decimal TpEva2, decimal TpEva3, decimal NotaDic, decimal NotaMar,
        int TotHoras, int Inasist, int Justif);

    private sealed record Efecto(
        bool CursadaExiste, int HstCount, string? HstCondicion, string? HstCondant,
        decimal? HstTpEva2, decimal? AnaNota, DateTime? AnaFecha, string? AnaCondicion, string? AnaMatriz);

    // Copia la cursada real al staging (pass-through = identidad) y fija las notas del escenario
    // + CONDICION/NOTAFIN ya resueltos, para correr solo el commit XXX_REGULARIZACION.
    private static async Task PoblarStagingRegularAsync(
        FbConnection conn, FbTransaction tx, CursadaRef c, Escenario333 e)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM \"$$$CURSADA\" WHERE USUARIO = @U", new { U = UsuarioPrueba }, tx));
        await conn.ExecuteAsync(new CommandDefinition("""
            INSERT INTO "$$$CURSADA" (USUARIO, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, CONDICION,
                                      TP_EVA, TP_EVA2, TP_EVA3, NOTADIC, NOTAMAR, FECHDIC, FECHMAR,
                                      FEC_EVA1, FEC_EVA2, FEC_EVA3, TOT_HORAS, INASIST, JUSTIF, FECHA1,
                                      NOTAFIN, NOTAFIN_FECHA,
                                      RECUP, RECUP2, REGULAR, FINAL1, FINAL2, FECHA2, PROM,
                                      FAL_EVA1, FAL_EVA2, FAL_EVA3, APELLIDO, MATRIZ)
            SELECT @U, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, 'REGULAR',
                   @TpEva, @TpEva2, @TpEva3, @NotaDic, @NotaMar, NULL, NULL,
                   NULL, @FecEva2, NULL, @TotHoras, @Inasist, @Justif, @FecEva2,
                   8, @FecEva2,
                   RECUP, RECUP2, REGULAR, FINAL1, FINAL2, FECHA2, PROM,
                   FAL_EVA1, FAL_EVA2, FAL_EVA3, APELLIDO, MATRIZ
            FROM CURSADA
            WHERE CARRE = @Carre AND COD_ALU = @A AND COD_MAT = @M AND CUA_ANIO = @Cua
            """,
            new
            {
                U = UsuarioPrueba, c.Carre, A = c.CodAlu, M = c.CodMat, Cua = c.CuaAnio,
                e.TpEva, e.TpEva2, e.TpEva3, e.NotaDic, e.NotaMar, FecEva2,
                e.TotHoras, e.Inasist, e.Justif,
            }, tx));
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
                SELECT FIRST 1 TRIM(CONDICION) AS CONDICION, TRIM(CONDANT) AS CONDANT, TP_EVA2
                FROM CURSADA_HST WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat ORDER BY INDICE DESC
                """, p, tx));
            var ana = await connection.QueryFirstOrDefaultAsync(new CommandDefinition("""
                SELECT NOTA_MAT, FEC_FINAL, TRIM(CONDICION) AS CONDICION, TRIM(MATRIZ) AS MATRIZ
                FROM ANALITIC WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
                """, p, tx));

            return new Efecto(
                cursadaExiste, hstCount,
                (string?)hst?.CONDICION, (string?)hst?.CONDANT, (decimal?)hst?.TP_EVA2,
                (decimal?)ana?.NOTA_MAT, (DateTime?)ana?.FEC_FINAL, (string?)ana?.CONDICION, (string?)ana?.MATRIZ);
        }
        finally
        {
            await tx.RollbackAsync(CancellationToken.None);
        }
    }
}
