using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Tests de las queries de comisiones y docentes (hito 6) contra Firebird real.
/// Paridad con el SELECT de cargacomisiones.FormActivate (COMARM + LEFT JOIN
/// MATERIAS + LEFT JOIN DOCENTES) más paginación/orden/filtros server-side.
/// </summary>
[Trait("Category", "Integration")]
public class ComisionesQueryTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static ComisionesQuery CrearQuery() => new(new FbConnectionFactory(ConnectionString));

    private static DocentesQuery CrearDocentesQuery() => new(new FbConnectionFactory(ConnectionString));

    private static EsbaDbContext CrearContexto() => new(Opciones);

    /// <summary>Carrera+cuatrimestre/año con comisiones cargadas (la de mayor cantidad).</summary>
    private static async Task<(string Carrera, string CuaAnio)?> GrupoConComisionesAsync(CancellationToken ct)
    {
        await using var contexto = CrearContexto();
        var claves = await contexto.Comisiones.AsNoTracking()
            .Select(c => new { c.CodigoCarrera, c.CuatrimestreAnio })
            .ToListAsync(ct);

        if (claves.Count == 0)
        {
            return null;
        }

        var grupo = claves
            .GroupBy(c => (c.CodigoCarrera, c.CuatrimestreAnio))
            .OrderByDescending(g => g.Count())
            .First().Key;

        return (grupo.CodigoCarrera, grupo.CuatrimestreAnio);
    }

    [Fact]
    public async Task Buscar_PorCarreraYCuatrimestre_DevuelveComisionesDeEseGrupo()
    {
        var ct = CancellationToken.None;
        var grupo = await GrupoConComisionesAsync(ct);
        if (grupo is null)
        {
            return; // sin comisiones cargadas; nada que verificar.
        }

        var resultado = await CrearQuery().BuscarAsync(
            new ComisionesFiltro { CodigoCarrera = grupo.Value.Carrera, CuatrimestreAnio = grupo.Value.CuaAnio, Take = 5 },
            ct);

        Assert.NotEmpty(resultado.Items);
        Assert.True(resultado.Items.Count <= 5);
        Assert.True(resultado.Total >= resultado.Items.Count);
        Assert.All(resultado.Items, c =>
        {
            Assert.Equal(grupo.Value.Carrera, c.CodigoCarrera);
            Assert.Equal(grupo.Value.CuaAnio, c.CuatrimestreAnio);
        });
    }

    [Fact]
    public async Task Buscar_Paginacion_DevuelvePaginasDistintasYEstables()
    {
        var ct = CancellationToken.None;
        var grupo = await GrupoConComisionesAsync(ct);
        if (grupo is null)
        {
            return;
        }

        var query = CrearQuery();
        var filtro = new ComisionesFiltro
        {
            CodigoCarrera = grupo.Value.Carrera,
            CuatrimestreAnio = grupo.Value.CuaAnio,
            Take = 1,
        };

        var pagina1 = await query.BuscarAsync(filtro, ct);
        var pagina1Repetida = await query.BuscarAsync(filtro, ct);
        var pagina2 = await query.BuscarAsync(filtro with { Skip = 1 }, ct);

        if (pagina1.Total < 2)
        {
            return; // el grupo tiene una sola comisión; la paginación no aplica.
        }

        Assert.Equal(
            pagina1.Items.Select(c => (c.Cutuco, c.CodigoMateria)),
            pagina1Repetida.Items.Select(c => (c.Cutuco, c.CodigoMateria)));
        Assert.Empty(pagina1.Items.Select(c => (c.Cutuco, c.CodigoMateria))
            .Intersect(pagina2.Items.Select(c => (c.Cutuco, c.CodigoMateria))));
    }

    [Fact]
    public async Task Buscar_OrdenInvalido_NoRompe()
    {
        var ct = CancellationToken.None;
        var grupo = await GrupoConComisionesAsync(ct);
        if (grupo is null)
        {
            return;
        }

        var resultado = await CrearQuery().BuscarAsync(
            new ComisionesFiltro
            {
                CodigoCarrera = grupo.Value.Carrera,
                CuatrimestreAnio = grupo.Value.CuaAnio,
                OrdenarPor = "DROP TABLE",
                Take = 5,
            }, ct);

        Assert.NotEmpty(resultado.Items);
    }

    [Fact]
    public async Task ListarDocentesActivos_NoLanzaYTodosTienenCodigo()
    {
        var docentes = await CrearDocentesQuery().ListarActivosAsync(CancellationToken.None);

        Assert.All(docentes, d => Assert.False(string.IsNullOrWhiteSpace(d.Codigo)));
    }
}
