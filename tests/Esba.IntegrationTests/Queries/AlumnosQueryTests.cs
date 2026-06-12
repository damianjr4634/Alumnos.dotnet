using Esba.Application.DTOs.Alumnos;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Tests de la query del padrón contra la base Firebird real. SQL legacy de
/// referencia: FrmEsba.pas:527-553 (SELECT del padrón con JOIN CARRERA,
/// BARRA_SEGU condicional y filtro BAJA).
/// </summary>
[Trait("Category", "Integration")]
public class AlumnosQueryTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/var/firebird/esba_restore.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static AlumnosQuery CrearQuery() =>
        new(new FbConnectionFactory(ConnectionString));

    private static EsbaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<EsbaDbContext>()
            .UseFirebird(ConnectionString)
            .Options;
        return new EsbaDbContext(options);
    }

    [Fact]
    public async Task BuscarPadron_SupervisorSinFiltros_DevuelvePaginaYTotal()
    {
        var resultado = await CrearQuery().BuscarPadronAsync(
            new PadronAlumnosFiltro { EsSupervisor = true, Take = 10 },
            CancellationToken.None);

        Assert.NotEmpty(resultado.Items);
        Assert.True(resultado.Items.Count <= 10);
        Assert.True(resultado.Total >= resultado.Items.Count);
        Assert.All(resultado.Items, a => Assert.False(a.Baja));
    }

    [Fact]
    public async Task BuscarPadron_PorApellidoDeUnAlumnoReal_LoEncuentra()
    {
        var ct = CancellationToken.None;

        // Tomo un alumno activo real como referencia (equivalencia con el legacy:
        // el buscador global de FrmEsba lo encontraría por APELLIDO CONTAINING).
        await using var contexto = CrearContexto();
        var referencia = await contexto.Alumnos.AsNoTracking()
            .Where(a => !a.Baja && a.Apellido != null && a.Apellido.Trim() != "")
            .OrderBy(a => a.CodigoCarrera).ThenBy(a => a.Codigo)
            .FirstAsync(ct);

        var resultado = await CrearQuery().BuscarPadronAsync(
            new PadronAlumnosFiltro
            {
                EsSupervisor = true,
                Texto = referencia.Apellido!.Trim(),
                IncluirCarrerasEnDesuso = true,
                Take = 500,
            },
            ct);

        Assert.Contains(resultado.Items, a =>
            a.Codigo == referencia.Codigo.Trim() && a.CodigoCarrera == referencia.CodigoCarrera);
    }

    [Fact]
    public async Task BuscarPadron_Paginacion_DevuelvePaginasDistintasYEstables()
    {
        var ct = CancellationToken.None;
        var query = CrearQuery();
        var filtro = new PadronAlumnosFiltro { EsSupervisor = true, Take = 5 };

        var pagina1 = await query.BuscarPadronAsync(filtro, ct);
        var pagina1Repetida = await query.BuscarPadronAsync(filtro, ct);
        var pagina2 = await query.BuscarPadronAsync(filtro with { Skip = 5 }, ct);

        Assert.Equal(
            pagina1.Items.Select(a => (a.CodigoCarrera, a.Codigo)),
            pagina1Repetida.Items.Select(a => (a.CodigoCarrera, a.Codigo)));
        Assert.Empty(pagina1.Items.Select(a => (a.CodigoCarrera, a.Codigo))
            .Intersect(pagina2.Items.Select(a => (a.CodigoCarrera, a.Codigo))));
    }

    [Fact]
    public async Task BuscarPadron_BuscarBajas_SoloDevuelveDadosDeBaja()
    {
        var resultado = await CrearQuery().BuscarPadronAsync(
            new PadronAlumnosFiltro { EsSupervisor = true, BuscarDadosDeBaja = true, Take = 20 },
            CancellationToken.None);

        Assert.All(resultado.Items, a => Assert.True(a.Baja));
    }

    [Fact]
    public async Task BuscarPadron_NoSupervisorSinCodigoUsuario_Lanza()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CrearQuery().BuscarPadronAsync(
                new PadronAlumnosFiltro { EsSupervisor = false },
                CancellationToken.None));
    }
}
