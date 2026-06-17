using Dapper;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia de los wrappers de constancia (hito 9.1) contra la ejecución
/// directa de los SP en Firebird: la salida del wrapper debe coincidir con el
/// SELECT directo al SP, campo por campo (read-only, no muta datos).
/// </summary>
[Trait("Category", "Integration")]
public class ConstanciasEquivalenciaTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static async Task<(string Codigo, string Carrera)?> AlumnoRealAsync(CancellationToken ct)
    {
        await using var ctx = CrearContexto();
        var alumno = await ctx.Alumnos.AsNoTracking()
            .Where(a => !a.Baja)
            .OrderBy(a => a.CodigoCarrera).ThenBy(a => a.Codigo)
            .FirstOrDefaultAsync(ct);

        return alumno is null ? null : (alumno.Codigo.Trim(), alumno.CodigoCarrera.Trim());
    }

    [Fact]
    public async Task CertificadoEnTramite_CoincideConSpDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoRealAsync(ct) is not { } alumno)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var directo = await ctx.Database.GetDbConnection()
            .QueryFirstOrDefaultAsync<(int FErrCod, string? FErrMsg, int FCuatri)>(
                "SELECT FERRCOD, FERRMSG, FCUATRI FROM XXX_IMPRIME_CTT(@A, @C)",
                new { A = alumno.Codigo, C = alumno.Carrera });

        var wrapper = await new CertificadoEnTramiteProcedure(Factory)
            .VerificarAsync(alumno.Codigo, alumno.Carrera, ct);

        var estadoEsperado = directo.FErrCod switch
        {
            2 => OperationStatus.Error,
            1 => OperationStatus.NeedsConfirmation,
            _ => OperationStatus.Ok,
        };
        Assert.Equal(estadoEsperado, wrapper.Status);
        if (wrapper.Status != OperationStatus.Error)
        {
            Assert.Equal(directo.FCuatri, wrapper.Value);
        }
    }

    [Fact]
    public async Task PaseAlumno_CoincideConSpDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoRealAsync(ct) is not { } alumno)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var directo = await ctx.Database.GetDbConnection()
            .QueryFirstOrDefaultAsync<(int FErrCod, string? FErrMsg)>(
                "SELECT FERRCOD, FERRMSG FROM XXX_IMPRIME_PASE(@A, @C)",
                new { A = alumno.Codigo, C = alumno.Carrera });

        var wrapper = await new PaseAlumnoProcedure(Factory).VerificarAsync(alumno.Codigo, alumno.Carrera, ct);

        var estadoEsperado = directo.FErrCod == 2 ? OperationStatus.Error : OperationStatus.Ok;
        Assert.Equal(estadoEsperado, wrapper.Status);
    }

    [Fact]
    public async Task ParrafoConstancia_CoincideConSpDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoRealAsync(ct) is not { } alumno)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var directo = await ctx.Database.GetDbConnection()
            .QueryFirstOrDefaultAsync<string?>(
                "SELECT FPARRAFO FROM XXX_PARRAFO_CONSTANCIA(@A, @C, 'PASE')",
                new { A = alumno.Codigo, C = alumno.Carrera });

        var wrapper = await new ParrafoConstanciaProcedure(Factory)
            .ObtenerAsync(alumno.Codigo, alumno.Carrera, "PASE", ct);

        Assert.Equal(directo ?? string.Empty, wrapper);
    }

    [Fact]
    public async Task PromedioGeneral_CoincideConSpDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoRealAsync(ct) is not { } alumno)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var directo = await ctx.Database.GetDbConnection()
            .ExecuteScalarAsync<decimal?>(
                "SELECT PROMGRAL FROM XXX_PROMEDIO_GRAL(@A, @C)",
                new { A = alumno.Codigo, C = alumno.Carrera });

        var wrapper = await new PromedioGeneralProcedure(Factory)
            .ObtenerAsync(alumno.Codigo, alumno.Carrera, ct);

        Assert.Equal(directo ?? 0m, wrapper);
    }

    [Fact]
    public async Task ConstanciaMaterias_CuentaCoincideConSpDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoRealAsync(ct) is not { } alumno)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var totalDirecto = await ctx.Database.GetDbConnection()
            .ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM XXX_CONSTANCIA_TERCIARIA(@A, @C)",
                new { A = alumno.Codigo, C = alumno.Carrera });

        var wrapper = await new ConstanciaMateriasProcedure(Factory)
            .ListarAsync(alumno.Codigo, alumno.Carrera, ct);

        Assert.Equal(totalDirecto, wrapper.Count);
    }
}
