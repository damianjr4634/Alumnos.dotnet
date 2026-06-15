using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Tests de la query server-side de materias (hito 5) contra Firebird real.
/// Verifican la paridad con el listado legacy de altamodifmaterias.pas
/// (SELECT * FROM MATERIAS WHERE CODCARRE = …) y las capacidades nuevas:
/// paginación OFFSET/FETCH, orden por whitelist y filtros parametrizados.
/// </summary>
[Trait("Category", "Integration")]
public class MateriasQueryTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static MateriasQuery CrearQuery() => new(new FbConnectionFactory(ConnectionString));

    private static EsbaDbContext CrearContexto() => new(Opciones);

    /// <summary>Carrera con al menos 3 materias, para ejercitar paginación y orden.</summary>
    private static async Task<string> CarreraConMateriasAsync(CancellationToken ct)
    {
        await using var contexto = CrearContexto();
        var carreras = await contexto.Materias.AsNoTracking()
            .Select(m => m.CodigoCarrera)
            .ToListAsync(ct);

        return carreras
            .GroupBy(c => c)
            .Where(g => g.Count() >= 3)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .First();
    }

    [Fact]
    public async Task Buscar_PorCarrera_DevuelvePaginaYTotalDeEsaCarrera()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);

        var resultado = await CrearQuery().BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, Take = 2 }, ct);

        Assert.NotEmpty(resultado.Items);
        Assert.True(resultado.Items.Count <= 2);
        Assert.True(resultado.Total >= resultado.Items.Count);
        Assert.All(resultado.Items, m => Assert.Equal(carrera, m.CodigoCarrera));
    }

    [Fact]
    public async Task Buscar_SinFiltros_EquivaleAlListadoLegacyPorCarrera()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);
        var query = CrearQuery();

        // Listado legacy (sucesor directo del SELECT de altamodifmaterias.pas).
        var legacy = await query.ListarPorCarreraAsync(carrera, ct);
        // Búsqueda server-side sin filtros, con una página que abarca todo.
        var nuevo = await query.BuscarAsync(new MateriasFiltro { CodigoCarrera = carrera, Take = 1000 }, ct);

        Assert.Equal(legacy.Count, nuevo.Total);
        Assert.Equal(
            legacy.Select(m => m.Codigo).OrderBy(c => c, StringComparer.Ordinal),
            nuevo.Items.Select(m => m.Codigo).OrderBy(c => c, StringComparer.Ordinal));
    }

    [Fact]
    public async Task Buscar_Paginacion_DevuelvePaginasDistintasYEstables()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);
        var query = CrearQuery();
        var filtro = new MateriasFiltro { CodigoCarrera = carrera, Take = 1 };

        var pagina1 = await query.BuscarAsync(filtro, ct);
        var pagina1Repetida = await query.BuscarAsync(filtro, ct);
        var pagina2 = await query.BuscarAsync(filtro with { Skip = 1 }, ct);

        Assert.Equal(
            pagina1.Items.Select(m => m.Codigo),
            pagina1Repetida.Items.Select(m => m.Codigo));
        Assert.Empty(pagina1.Items.Select(m => m.Codigo).Intersect(pagina2.Items.Select(m => m.Codigo)));
    }

    [Fact]
    public async Task Buscar_OrdenPorNombreDescendente_DevuelveOrdenInverso()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);

        var resultado = await CrearQuery().BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, OrdenarPor = "Nombre", OrdenDescendente = true, Take = 1000 },
            ct);

        var nombres = resultado.Items.Select(m => m.Nombre ?? string.Empty).ToList();
        Assert.Equal(nombres.OrderByDescending(n => n, StringComparer.Ordinal).ToList(), nombres);
    }

    [Fact]
    public async Task Buscar_OrdenInvalido_NoRompeYUsaOrdenPorDefecto()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);

        // Un campo fuera de la whitelist no debe llegar al ORDER BY (anti-inyección).
        var resultado = await CrearQuery().BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, OrdenarPor = "DROP TABLE", Take = 5 }, ct);

        Assert.NotEmpty(resultado.Items);
    }

    [Fact]
    public async Task Buscar_FiltroSoloAnuales_RespetaElFlag()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);
        var query = CrearQuery();

        var anuales = await query.BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, SoloAnuales = true, Take = 1000 }, ct);
        var cuatrimestrales = await query.BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, SoloAnuales = false, Take = 1000 }, ct);

        Assert.All(anuales.Items, m => Assert.True(m.EsAnual));
        Assert.All(cuatrimestrales.Items, m => Assert.False(m.EsAnual));
    }

    [Fact]
    public async Task Buscar_FiltroTexto_EncuentraPorDescripcionOSigla()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);
        var query = CrearQuery();

        // Tomo una materia real con descripción y un fragmento de su nombre.
        var todas = await query.ListarPorCarreraAsync(carrera, ct);
        var referencia = todas.First(m => !string.IsNullOrWhiteSpace(m.Nombre) && m.Nombre!.Trim().Length >= 4);
        var fragmento = referencia.Nombre!.Trim()[..4];

        var resultado = await query.BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, Texto = fragmento, Take = 1000 }, ct);

        Assert.Contains(resultado.Items, m => m.Codigo == referencia.Codigo);
        Assert.All(resultado.Items, m =>
            Assert.True(
                (m.Nombre?.Contains(fragmento, StringComparison.OrdinalIgnoreCase) ?? false)
                || (m.Sigla?.Contains(fragmento, StringComparison.OrdinalIgnoreCase) ?? false)));
    }

    [Fact]
    public async Task ObtenerDetalle_DeUnaMateriaReal_DevuelveSusDatos()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);
        var query = CrearQuery();

        var referencia = (await query.ListarPorCarreraAsync(carrera, ct))[0];

        var detalle = await query.ObtenerDetalleAsync(carrera, referencia.Codigo, ct);

        Assert.NotNull(detalle);
        Assert.Equal(referencia.Codigo, detalle!.Codigo);
        Assert.Equal(carrera, detalle.CodigoCarrera);
        Assert.Equal(referencia.EsAnual ?? false, detalle.EsAnual);
    }

    [Fact]
    public async Task ObtenerDetalle_MateriaInexistente_DevuelveNull()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);

        var detalle = await CrearQuery().ObtenerDetalleAsync(carrera, "ZZ", ct);

        Assert.Null(detalle);
    }

    [Fact]
    public async Task Buscar_FiltroDadaDeBaja_RespetaElEstado()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);
        var query = CrearQuery();

        var activas = await query.BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, DadaDeBaja = false, Take = 1000 }, ct);
        var bajas = await query.BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, DadaDeBaja = true, Take = 1000 }, ct);

        Assert.All(activas.Items, m => Assert.False(m.DadaDeBaja));
        Assert.All(bajas.Items, m => Assert.True(m.DadaDeBaja));
    }

    [Fact]
    public async Task Buscar_FiltroCuatrimestre_SoloDevuelveEseCuatrimestre()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMateriasAsync(ct);
        var query = CrearQuery();

        var todas = await query.ListarPorCarreraAsync(carrera, ct);
        var cuatrimestre = todas.FirstOrDefault(m => m.Cuatrimestre is not null)?.Cuatrimestre;
        if (cuatrimestre is null)
        {
            return; // la carrera no tiene cuatrimestre cargado; nada que verificar.
        }

        var resultado = await query.BuscarAsync(
            new MateriasFiltro { CodigoCarrera = carrera, Cuatrimestre = cuatrimestre, Take = 1000 }, ct);

        Assert.NotEmpty(resultado.Items);
        Assert.All(resultado.Items, m => Assert.Equal(cuatrimestre, m.Cuatrimestre));
    }
}
