using Dapper;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Tests del listado de mesas de examen (hito 8) contra Firebird real: paridad
/// con el SELECT de MesasExamen.FormActivate + paginación/orden server-side.
/// </summary>
[Trait("Category", "Integration")]
public class MesasQueryTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static async Task<string?> CarreraConMesasAsync()
    {
        await using var conn = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        return await conn.ExecuteScalarAsync<string?>("SELECT FIRST 1 TRIM(CARRE) FROM MESAS ORDER BY CARRE");
    }

    [Fact]
    public async Task Buscar_PorCarrera_DevuelveMesasDeEsaCarrera()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMesasAsync();
        if (carrera is null)
        {
            return; // sin mesas cargadas.
        }

        var resultado = await new MesasQuery(Factory).BuscarAsync(
            new MesasFiltro { CodigoCarrera = carrera, Take = 5 }, ct);

        Assert.NotEmpty(resultado.Items);
        Assert.True(resultado.Items.Count <= 5);
        Assert.True(resultado.Total >= resultado.Items.Count);
        Assert.All(resultado.Items, m => Assert.Equal(carrera, m.CodigoCarrera));
    }

    [Fact]
    public async Task Buscar_OrdenInvalido_NoRompe()
    {
        var ct = CancellationToken.None;
        var carrera = await CarreraConMesasAsync();
        if (carrera is null)
        {
            return;
        }

        var resultado = await new MesasQuery(Factory).BuscarAsync(
            new MesasFiltro { CodigoCarrera = carrera, OrdenarPor = "DROP TABLE", Take = 5 }, ct);

        Assert.NotEmpty(resultado.Items);
    }
}
