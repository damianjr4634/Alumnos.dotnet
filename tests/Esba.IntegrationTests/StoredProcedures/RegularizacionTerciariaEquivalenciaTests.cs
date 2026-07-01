using Dapper;
using Esba.Domain.Academica;
using Esba.Infrastructure.Persistence;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia del cálculo de condición terciaria portado a C#
/// (<see cref="CalculoCondicionRegularizacionTerciaria"/>) contra el SP legacy
/// XXX_REGULARIZACION_MAT_TERC (Prompt 4.B). Para varias combinaciones de notas se
/// puebla el staging "$$$CURSADA", se corre el SP y se compara la CONDICION que deja
/// contra la que resuelve el dominio. Todo dentro de una transacción que se revierte:
/// la base no se muta.
/// </summary>
[Trait("Category", "Integration")]
public class RegularizacionTerciariaEquivalenciaTests
{
    private const int UsuarioPrueba = 9998;

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
        public string Condicion { get; init; } = string.Empty;
        public int Promociona { get; init; }
        public int ApruebaSinFinal { get; init; }
    }

    private sealed record Escenario(decimal? TpEva, decimal? TpEva2, decimal? Recup, int TotHoras, int Inasist, int Justif);

    [Fact]
    public async Task Condicion_DominioYSp_Coinciden_ParaVariasNotas()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        var cursada = await connection.QueryFirstOrDefaultAsync<CursadaRef>("""
            SELECT FIRST 1 TRIM(C.CARRE) AS Carre, TRIM(C.COD_ALU) AS CodAlu, TRIM(C.COD_MAT) AS CodMat,
                   C.CUTUCO AS Cutuco, TRIM(C.CUA_ANIO) AS CuaAnio, TRIM(C.CONDICION) AS Condicion,
                   IIF(TRIM(COALESCE(M.PROMOCION, 'N')) = 'S', 1, 0)  AS Promociona,
                   IIF(TRIM(COALESCE(M.APRSFINAL, 'N')) = 'S', 1, 0)  AS ApruebaSinFinal
            FROM CURSADA C
            INNER JOIN CARRERA R ON R.CARRE = C.CARRE
            LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = C.COD_MAT AND M.CODCARRE = C.CARRE
            WHERE R.TIPO = 'TER' AND COALESCE(TRIM(C.CUA_ANIO), '') <> ''
            """);

        Assert.True(cursada is not null, "Se necesita una cursada terciaria para la prueba.");

        var notaPromocionTexto = await connection.ExecuteScalarAsync<string?>(
            "SELECT COALESCE(VALOR, '0') FROM XXX_CONF WHERE PARAME = 'Regula_NotPromocion'");
        var notaPromocion = decimal.TryParse(notaPromocionTexto, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var np) ? np : 0m;

        var escenarios = new[]
        {
            new Escenario(7m, 6m, null, 100, 0, 0),     // dos parciales aprobados
            new Escenario(3m, 5m, 7m, 100, 0, 0),       // recuperatorio aprueba
            new Escenario(2m, 3m, 3m, 100, 0, 0),       // recuperatorio no alcanza
            new Escenario(99m, 99m, 99m, 100, 0, 0),    // ausentes
            new Escenario(8m, 8m, null, 100, 70, 0),    // muchas inasistencias
            new Escenario(8m, 8m, null, 100, 45, 0),    // inasistencias moderadas
        };

        await using var tx = (FbTransaction)await connection.BeginTransactionAsync(CancellationToken.None);
        try
        {
            foreach (var e in escenarios)
            {
                var condicionSp = await CondicionPorSpAsync(connection, tx, cursada!, e);

                var condicionDominio = CalculoCondicionRegularizacionTerciaria.ResolverCondicion(
                    new NotasRegularizacionTerciaria(
                        cursada!.Condicion, e.TpEva, e.TpEva2, e.Recup, e.TotHoras, e.Inasist, e.Justif,
                        MateriaPromociona: cursada.Promociona == 1,
                        MateriaApruebaSinFinal: cursada.ApruebaSinFinal == 1),
                    notaPromocion);

                Assert.Equal(condicionSp, condicionDominio);
            }
        }
        finally
        {
            await tx.RollbackAsync(CancellationToken.None);
        }
    }

    private static async Task<string?> CondicionPorSpAsync(
        FbConnection connection, FbTransaction tx, CursadaRef c, Escenario e)
    {
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM \"$$$CURSADA\" WHERE USUARIO = @Usuario", new { Usuario = UsuarioPrueba }, tx));

        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO "$$$CURSADA" (USUARIO, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, CONDICION,
                                      TP_EVA, TP_EVA2, RECUP, TOT_HORAS, INASIST, JUSTIF)
            VALUES (@Usuario, @CodAlu, @CodMat, @Cutuco, @CuaAnio, @Condicion,
                    @TpEva, @TpEva2, @Recup, @TotHoras, @Inasist, @Justif)
            """,
            new
            {
                Usuario = UsuarioPrueba,
                c.CodAlu,
                c.CodMat,
                c.Cutuco,
                c.CuaAnio,
                c.Condicion,
                e.TpEva,
                e.TpEva2,
                e.Recup,
                e.TotHoras,
                e.Inasist,
                e.Justif,
            },
            tx));

        // El SP es seleccionable: hay que traer la fila para que corra el cuerpo.
        _ = await connection.QueryFirstOrDefaultAsync<(int? FErrCod, string? FErrMsg)>(new CommandDefinition(
            "SELECT FERRCOD, FERRMSG FROM XXX_REGULARIZACION_MAT_TERC(@Carre, @CodAlu, @CodMat, @CondOrig, @Usuario)",
            new { c.Carre, c.CodAlu, c.CodMat, CondOrig = c.Condicion, Usuario = UsuarioPrueba },
            tx));

        return await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
            "SELECT TRIM(CONDICION) FROM \"$$$CURSADA\" WHERE USUARIO = @Usuario AND COD_MAT = @CodMat",
            new { Usuario = UsuarioPrueba, c.CodMat }, tx));
    }
}
