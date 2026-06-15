using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Asistencias;

/// <summary>
/// Tests de la capa de lectura de asistencias (hito 7, increment 1) contra
/// Firebird real: catálogo TBL_FALTAS y wrappers XXX_FALTAS_COMISION /
/// XXX_FALTAS_FALTAS. Solo lectura.
/// </summary>
[Trait("Category", "Integration")]
public class AsistenciasQueryTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    [Fact]
    public async Task TiposFalta_MapeoEf_LeeElCatalogo()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();

        var tipos = await ctx.TiposFalta.AsNoTracking().Take(5).ToListAsync(ct);

        Assert.All(tipos, t => Assert.False(string.IsNullOrWhiteSpace(t.Codigo)));
    }

    [Fact]
    public async Task ListarTiposPorCarrera_DevuelveAplicables()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();
        var carrera = await ctx.Materias.AsNoTracking().Select(m => m.CodigoCarrera).FirstOrDefaultAsync(ct);
        if (carrera is null)
        {
            return;
        }

        var tipos = await new TipoFaltasQuery(Factory).ListarPorCarreraAsync(carrera, ct);

        Assert.All(tipos, t => Assert.False(string.IsNullOrWhiteSpace(t.Codigo)));
    }

    [Fact]
    public async Task FaltasComisionYAlumno_NoLanzanYMapean()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();
        var comision = await ctx.Comisiones.AsNoTracking()
            .OrderBy(c => c.CodigoCarrera).ThenBy(c => c.Cutuco)
            .FirstOrDefaultAsync(ct);
        if (comision is null)
        {
            return;
        }

        var alumnos = await new FaltasComisionProcedure(Factory).ListarAsync(
            comision.CodigoCarrera, comision.Cutuco, comision.CuatrimestreAnio, comision.CodigoMateria, ct);

        Assert.All(alumnos, a => Assert.False(string.IsNullOrWhiteSpace(a.CodigoAlumno)));

        if (alumnos.Count > 0)
        {
            var faltas = await new FaltasAlumnoProcedure(Factory).ListarAsync(
                comision.CodigoCarrera, comision.Cutuco, comision.CuatrimestreAnio,
                alumnos[0].CodigoAlumno, comision.CodigoMateria, ct);

            Assert.All(faltas, f => Assert.False(string.IsNullOrWhiteSpace(f.CodigoFalta)));
        }
    }
}
