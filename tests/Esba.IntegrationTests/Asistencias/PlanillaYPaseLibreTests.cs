using Dapper;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Asistencias;

/// <summary>
/// Tests de planilla (XXX_FALTAS_IMPRESI, read-only) y de la previsualización del
/// pase a LIBRE (XXX_FALTAS_PASLIBRE con rollback ⇒ no muta) contra Firebird real.
/// </summary>
[Trait("Category", "Integration")]
public class PlanillaYPaseLibreTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    [Fact]
    public async Task Planilla_NoLanzaYMapea()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();
        var carrera = await ctx.Materias.AsNoTracking().Select(m => m.CodigoCarrera).FirstOrDefaultAsync(ct);
        if (carrera is null)
        {
            return;
        }

        var filas = await new PlanillaInasistenciasProcedure(Factory)
            .ListarAsync(carrera, "0", "124", 5m, 10m, 15m, ct);

        Assert.All(filas, f => Assert.False(string.IsNullOrWhiteSpace(f.CodigoAlumno)));
    }

    [Fact]
    public async Task PaseLibrePreview_HaceRollback_NoCambiaCursando()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();

        // Un alumno con materias CURSANDO, si lo hay.
        var cursando = await ctx.Cursadas.AsNoTracking()
            .Where(c => c.Condicion == "CURSANDO")
            .OrderBy(c => c.CodigoCarrera).ThenBy(c => c.CodigoAlumno)
            .FirstOrDefaultAsync(ct);
        if (cursando is null)
        {
            return;
        }

        var carre = cursando.CodigoCarrera.Trim();
        var codAlu = cursando.CodigoAlumno.Trim();

        var antes = await ContarCursandoAsync(carre, codAlu);
        var resultado = await new PaseLibreProcedure(Factory).EjecutarAsync(codAlu, carre, confirmar: false, ct);
        var despues = await ContarCursandoAsync(carre, codAlu);

        Assert.Equal(OperationStatus.NeedsConfirmation, resultado.Status);
        Assert.Equal(antes, despues); // el rollback dejó las materias como estaban
    }

    private static async Task<int> ContarCursandoAsync(string carre, string codAlu)
    {
        await using var ctx = CrearContexto();
        var conn = ctx.Database.GetDbConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CURSADA WHERE CARRE=@C AND COD_ALU=@A AND CONDICION='CURSANDO'",
            new { C = carre, A = codAlu });
    }
}
