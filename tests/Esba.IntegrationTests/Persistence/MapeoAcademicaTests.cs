using Esba.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Persistence;

/// <summary>
/// Smoke tests del mapeo EF Core del área Académica contra la base real:
/// materializan entidades completas para detectar columnas mal mapeadas o
/// tipos incompatibles.
/// </summary>
[Trait("Category", "Integration")]
public class MapeoAcademicaTests
{
    private static EsbaDbContext CrearContexto()
    {
        var connectionString = Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
            ?? "database=localhost:/var/firebird/esba_restore.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

        var options = new DbContextOptionsBuilder<EsbaDbContext>()
            .UseFirebird(connectionString)
            .Options;

        return new EsbaDbContext(options);
    }

    [Fact]
    public async Task Materias_MaterializaConCarrera()
    {
        await using var contexto = CrearContexto();

        var materias = await contexto.Materias.AsNoTracking()
            .Include(m => m.Carrera)
            .OrderBy(m => m.CodigoCarrera).ThenBy(m => m.Codigo)
            .Take(200)
            .ToListAsync();

        Assert.NotEmpty(materias);
        Assert.All(materias, m => Assert.False(string.IsNullOrWhiteSpace(m.Codigo)));
    }

    [Fact]
    public async Task Comisiones_MaterializaConMateria()
    {
        await using var contexto = CrearContexto();

        var comisiones = await contexto.Comisiones.AsNoTracking()
            .Include(c => c.Materia)
            .OrderBy(c => c.CodigoCarrera).ThenBy(c => c.CuatrimestreAnio)
            .Take(200)
            .ToListAsync();

        Assert.NotEmpty(comisiones);
    }

    [Fact]
    public async Task Cursadas_MaterializaEntidadesCompletas()
    {
        await using var contexto = CrearContexto();

        var cursadas = await contexto.Cursadas.AsNoTracking()
            .OrderBy(c => c.CodigoCarrera).ThenBy(c => c.CodigoAlumno).ThenBy(c => c.CodigoMateria)
            .Take(500)
            .ToListAsync();

        Assert.NotEmpty(cursadas);
        Assert.All(cursadas, c => Assert.False(string.IsNullOrWhiteSpace(c.Condicion)));
    }

    [Fact]
    public async Task Cursadas_JoinImplicitoConMateria_Funciona()
    {
        await using var contexto = CrearContexto();

        var conMateria = await contexto.Cursadas.AsNoTracking()
            .Include(c => c.Materia)
            .Where(c => c.Condicion.Trim() == "REGULAR")
            .OrderBy(c => c.CodigoCarrera).ThenBy(c => c.CodigoAlumno).ThenBy(c => c.CodigoMateria)
            .Take(50)
            .ToListAsync();

        Assert.NotEmpty(conMateria);
    }

    [Fact]
    public async Task CiclosLectivos_Materializan()
    {
        await using var contexto = CrearContexto();

        var cuatrimestrales = await contexto.CiclosCuatrimestrales.AsNoTracking().ToListAsync();
        var trimestrales = await contexto.CiclosTrimestrales.AsNoTracking().ToListAsync();

        Assert.NotEmpty(cuatrimestrales);
        Assert.All(cuatrimestrales, c => Assert.True(c.PrimerCuatrimestreDesde < c.SegundoCuatrimestreHasta));
        Assert.NotEmpty(trimestrales);
    }
}
