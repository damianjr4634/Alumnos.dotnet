using Dapper;
using Esba.Domain.Academica;
using Esba.Infrastructure.Persistence;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia del cálculo de condición de bachillerato portado a C#
/// (<see cref="CalculoCondicionRegularizacionBachiller"/>) contra los SP legacy
/// XXX_REGULARIZACION_MAT_BAC (faltas) + XXX_REGULARIZACION_MAT_POSTVAL (notas, la carrera
/// 'BAC'). Para varias combinaciones se puebla el staging "$$$CURSADA" —incluidos los
/// derivados TP_EVA3/FINAL1 que el formulario legacy calculaba en la UI—, se corren ambos
/// SP y se compara la CONDICION (y la nota final) que dejan contra las que resuelve el
/// dominio. Todo dentro de una transacción que se revierte: la base no se muta.
/// </summary>
[Trait("Category", "Integration")]
public class RegularizacionBachilleratoEquivalenciaTests
{
    private const int UsuarioPrueba = 9997;
    private const string CondicionOrigen = "CURSANDO";

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
        public int EnRecursa { get; init; }
    }

    private sealed record Escenario(
        decimal? TpEva, decimal? TpEva2, decimal? Recup, decimal? Regular, int TotHoras, int Inasist);

    [Fact]
    public async Task CondicionYNotaFinal_DominioYSp_Coinciden_ParaVariasNotas()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        // La carrera 'BAC' es la única que corre el ladder de notas (_POSTVAL tiene guard CARRE='BAC').
        var cursada = await connection.QueryFirstOrDefaultAsync<CursadaRef>("""
            SELECT FIRST 1 TRIM(C.CARRE) AS Carre, TRIM(C.COD_ALU) AS CodAlu, TRIM(C.COD_MAT) AS CodMat,
                   C.CUTUCO AS Cutuco, TRIM(C.CUA_ANIO) AS CuaAnio,
                   IIF(EXISTS(SELECT 1 FROM RECURSA R WHERE R.COD_ALU = C.COD_ALU AND R.CARRE = C.CARRE
                              AND R.CUTUCO = C.CUTUCO AND R.COD_MAT = C.COD_MAT AND R.CUA_ANIO = C.CUA_ANIO), 1, 0) AS EnRecursa
            FROM CURSADA C
            WHERE C.CARRE = 'BAC' AND COALESCE(TRIM(C.CUA_ANIO), '') <> ''
            """);

        Assert.True(cursada is not null, "Se necesita una cursada de la carrera 'BAC' para la prueba.");

        var escenarios = new[]
        {
            new Escenario(7m, 8m, null, null, 100, 0),   // dos bimestres aprobados → REGULAR
            new Escenario(2m, 3m, 8m, null, 100, 0),      // promedio bajo, recuperatorio aprueba → REGULAR
            new Escenario(2m, 3m, 0m, 0m, 100, 0),        // sin recup ni nota a-regular → A/REGULAR
            new Escenario(2m, 3m, 0m, 4m, 100, 0),        // nota a-regular desaprobada → PREVIO
            new Escenario(2m, 3m, 0m, 7m, 100, 0),        // nota a-regular aprobada → REGULAR
            new Escenario(99m, 99m, 0m, 0m, 100, 0),      // ambos ausentes → LIBRES
            new Escenario(8m, 8m, 0m, 0m, 100, 50),       // muchas faltas → LIBRES
            new Escenario(8m, 8m, 0m, 0m, 0, 0),          // sin carga horaria → mantiene CURSANDO
            new Escenario(5m, 5m, 8m, 0m, 100, 0),        // promedio 5, recuperatorio aprueba → REGULAR
        };

        await using var tx = (FbTransaction)await connection.BeginTransactionAsync(CancellationToken.None);
        try
        {
            foreach (var e in escenarios)
            {
                var (condicionSp, notaFinalSp) = await EjecutarSpAsync(connection, tx, cursada!, e, paso: string.Empty);

                var resultado = CalculoCondicionRegularizacionBachiller.Resolver(Notas(cursada!, e, paso: null));

                Assert.Equal(condicionSp, resultado.Condicion);
                if (resultado.VaAlAnalitico)
                {
                    Assert.Equal(notaFinalSp, resultado.NotaFinal);
                }
            }

            // CONSEJO (faltas 26-40%): el SP deja 'CONSEJO' a la espera de decisión; el dominio pide decisión.
            var consejo = new Escenario(8m, 8m, 0m, 0m, 100, 30);
            var (condicionConsejoSp, _) = await EjecutarSpAsync(connection, tx, cursada!, consejo, paso: string.Empty);
            var resultadoConsejo = CalculoCondicionRegularizacionBachiller.Resolver(Notas(cursada!, consejo, paso: null));
            Assert.Equal("CONSEJO", condicionConsejoSp);
            Assert.True(resultadoConsejo.RequiereDecision);

            // CONSEJO + el operador elige "Regular": con dos bimestres altos, ambos → REGULAR.
            var (condicionRegularSp, _) = await EjecutarSpAsync(connection, tx, cursada!, consejo, paso: "Regular");
            var resultadoRegular = CalculoCondicionRegularizacionBachiller.Resolver(Notas(cursada!, consejo, paso: "Regular"));
            Assert.Equal(condicionRegularSp, resultadoRegular.Condicion);
        }
        finally
        {
            await tx.RollbackAsync(CancellationToken.None);
        }
    }

    private static NotasRegularizacionBachiller Notas(CursadaRef c, Escenario e, string? paso) =>
        new(CondicionOrigen, e.TpEva, e.TpEva2, e.Recup, e.Regular, e.TotHoras, e.Inasist,
            EnRecursa: c.EnRecursa == 1, paso);

    // Promedio (TP_EVA3) y nota definitiva (FINAL1) como los computaba el formulario legacy
    // antes de llamar a los SP: 99 (ausente) cuenta como 1; FINAL1 solo con recuperatorio.
    private static decimal PromedioPas(decimal? tp, decimal? tp2)
    {
        var a = tp ?? 0m;
        var b = tp2 ?? 0m;
        if (a == 99m || b == 99m)
        {
            return ((a == 99m ? 1m : a) + (b == 99m ? 1m : b)) / 2m;
        }

        return (a + b) / 2m;
    }

    private static async Task<(string? Condicion, decimal? NotaFinal)> EjecutarSpAsync(
        FbConnection connection, FbTransaction tx, CursadaRef c, Escenario e, string paso)
    {
        var promedio = PromedioPas(e.TpEva, e.TpEva2);
        decimal? final1 = (e.Recup ?? 0m) != 0m ? (promedio + (e.Recup ?? 0m)) / 2m : null;

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM \"$$$CURSADA\" WHERE USUARIO = @Usuario", new { Usuario = UsuarioPrueba }, tx));

        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO "$$$CURSADA" (USUARIO, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, CONDICION,
                                      TP_EVA, TP_EVA2, TP_EVA3, RECUP, REGULAR, TOT_HORAS, INASIST, JUSTIF, FINAL1)
            VALUES (@Usuario, @CodAlu, @CodMat, @Cutuco, @CuaAnio, @Condicion,
                    @TpEva, @TpEva2, @TpEva3, @Recup, @Regular, @TotHoras, @Inasist, 0, @Final1)
            """,
            new
            {
                Usuario = UsuarioPrueba,
                c.CodAlu,
                c.CodMat,
                c.Cutuco,
                c.CuaAnio,
                Condicion = CondicionOrigen,
                e.TpEva,
                e.TpEva2,
                TpEva3 = promedio,
                e.Recup,
                e.Regular,
                e.TotHoras,
                e.Inasist,
                Final1 = final1,
            },
            tx));

        // 1) _BAC (faltas): fija CONDICION en el staging por porcentaje de inasistencias.
        _ = await connection.QueryFirstOrDefaultAsync<(int? FErrCod, string? FErrMsg)>(new CommandDefinition(
            "SELECT FERRCOD, FERRMSG FROM XXX_REGULARIZACION_MAT_BAC(@Carre, @CodAlu, @CodMat, @CondOrig, @Usuario)",
            new { c.Carre, c.CodAlu, c.CodMat, CondOrig = CondicionOrigen, Usuario = UsuarioPrueba }, tx));

        // 2) _POSTVAL (notas): resuelve la condición final; con PASO decide el caso CONSEJO.
        _ = await connection.QueryFirstOrDefaultAsync<(int? FErrCod, string? FErrMsg, string? FButtons)>(new CommandDefinition(
            "SELECT FERRCOD, FERRMSG, FBUTTONS FROM XXX_REGULARIZACION_MAT_POSTVAL(@Carre, @CodAlu, @CodMat, @Usuario, @Paso)",
            new { c.Carre, c.CodAlu, c.CodMat, Usuario = UsuarioPrueba, Paso = paso }, tx));

        var fila = await connection.QueryFirstOrDefaultAsync<(string? Condicion, decimal? Final1)>(new CommandDefinition(
            "SELECT TRIM(CONDICION), FINAL1 FROM \"$$$CURSADA\" WHERE USUARIO = @Usuario AND COD_MAT = @CodMat",
            new { Usuario = UsuarioPrueba, c.CodMat }, tx));

        return (fila.Condicion, fila.Final1);
    }
}
