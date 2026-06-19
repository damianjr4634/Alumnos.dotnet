using Dapper;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia de los wrappers de equivalencias (hito 9.3b) contra la ejecución
/// directa de los SP en Firebird (read-only, no muta datos). No cubre
/// XXX_GRABA_NUMEQUI porque escribe en TBLEQUIVA (lo cubre el test unitario del handler).
/// </summary>
[Trait("Category", "Integration")]
public class EquivalenciaProcedureTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static async Task<(string Codigo, string Carrera, string Materia)?> AlumnoConMateriaAsync(CancellationToken ct)
    {
        await using var ctx = CrearContexto();
        var alumno = await ctx.Alumnos.AsNoTracking()
            .Where(a => !a.Baja)
            .OrderBy(a => a.CodigoCarrera).ThenBy(a => a.Codigo)
            .FirstOrDefaultAsync(ct);
        if (alumno is null)
        {
            return null;
        }

        var materia = await ctx.Materias.AsNoTracking()
            .Where(m => m.CodigoCarrera == alumno.CodigoCarrera)
            .OrderBy(m => m.Codigo)
            .FirstOrDefaultAsync(ct);

        return materia is null ? null : (alumno.Codigo.Trim(), alumno.CodigoCarrera.Trim(), materia.Codigo.Trim());
    }

    [Fact]
    public async Task ValidacionMateria_CoincideConSpDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoConMateriaAsync(ct) is not { } datos)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var directo = await ctx.Database.GetDbConnection()
            .QueryFirstOrDefaultAsync<(int FErrCod, string? FErrMsg)>(
                "SELECT FERRCOD, FERRMSG FROM XXX_INSC_VALMAT(@A, @C, @M, 'A')",
                new { A = datos.Codigo, C = datos.Carrera, M = datos.Materia });

        var wrapper = await new ValidacionMateriaProcedure(Factory)
            .ValidarAsync(datos.Codigo, datos.Carrera, datos.Materia, 'A', ct);

        var estadoEsperado = directo.FErrCod switch
        {
            2 => OperationStatus.Error,
            1 => OperationStatus.NeedsConfirmation,
            _ => OperationStatus.Ok,
        };
        Assert.Equal(estadoEsperado, wrapper.Status);
    }

    [Fact]
    public async Task NumeroEquivalencia_CoincideConSpDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoConMateriaAsync(ct) is not { } datos)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var directo = await ctx.Database.GetDbConnection()
            .QueryFirstOrDefaultAsync<(string? NumForma, string? NumEntero, string? FErrMsg, string? FNumNue)>(
                "SELECT NUM_FORMA, NUM_ENTERO, FERRMSG, FNUMNUE FROM XXX_NUMERO_EQUIVALENCIA(@A, @C)",
                new { A = datos.Codigo, C = datos.Carrera });

        var wrapper = await new EquivalenciaNumeracionProcedure(Factory)
            .ObtenerProximoNumeroAsync(datos.Codigo, datos.Carrera, ct);

        Assert.Equal(directo.NumEntero?.Trim() ?? string.Empty, wrapper.NumeroEntero);
        Assert.Equal(directo.NumForma?.Trim() ?? string.Empty, wrapper.NumeroFormateado);
        Assert.Equal(string.Equals(directo.FNumNue?.Trim(), "S", StringComparison.OrdinalIgnoreCase), wrapper.EsNuevo);
    }
}
