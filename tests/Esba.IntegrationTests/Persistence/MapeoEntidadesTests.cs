using Esba.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Persistence;

/// <summary>
/// Smoke tests del mapeo EF Core contra la base Firebird real: materializan
/// entidades completas para detectar columnas mal mapeadas, tipos incompatibles
/// o problemas de charset. Conexión por variable de entorno ESBA_TEST_CONNECTION
/// (default: base de desarrollo local).
/// </summary>
[Trait("Category", "Integration")]
public class MapeoEntidadesTests
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
    public async Task Carreras_MaterializaEntidadesCompletas()
    {
        await using var contexto = CrearContexto();

        var carreras = await contexto.Carreras.AsNoTracking().ToListAsync();

        Assert.NotEmpty(carreras);
        Assert.All(carreras, c => Assert.False(string.IsNullOrWhiteSpace(c.Codigo)));
    }

    [Fact]
    public async Task Alumnos_MaterializaEntidadesCompletas()
    {
        await using var contexto = CrearContexto();

        var alumnos = await contexto.Alumnos.AsNoTracking()
            .OrderBy(a => a.CodigoCarrera).ThenBy(a => a.Codigo)
            .Take(200)
            .ToListAsync();

        Assert.NotEmpty(alumnos);
        Assert.All(alumnos, a => Assert.False(string.IsNullOrWhiteSpace(a.Codigo)));
    }

    [Fact]
    public async Task Usuarios_MaterializaEntidadesConPermisos()
    {
        await using var contexto = CrearContexto();

        var usuarios = await contexto.Usuarios.AsNoTracking()
            .Include(u => u.Permisos)
            .ToListAsync();

        Assert.NotEmpty(usuarios);
        Assert.All(usuarios, u => Assert.False(string.IsNullOrWhiteSpace(u.NombreUsuario)));
        // Al menos un usuario no supervisor debe tener permisos en BARRA_SEGU
        // (de eso depende el filtro del padrón).
        Assert.Contains(usuarios, u => u.Permisos.Count > 0);
    }

    [Fact]
    public async Task Alumnos_JoinImplicitoConCarrera_Funciona()
    {
        await using var contexto = CrearContexto();

        var conCarrera = await contexto.Alumnos.AsNoTracking()
            .Include(a => a.Carrera)
            .OrderBy(a => a.CodigoCarrera).ThenBy(a => a.Codigo)
            .Take(20)
            .ToListAsync();

        Assert.NotEmpty(conCarrera);
        Assert.All(conCarrera, a => Assert.NotNull(a.Carrera));
    }
}
