using Dapper;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Lecturas del área docente contra Firebird real. Elige en la propia base un
/// docente con comisiones y otro con mesas como titular, y verifica que la query
/// devuelve exactamente las filas de ese docente (alcance de datos) con los joins
/// resueltos y el estado de precarga en null cuando no hay carga.
/// </summary>
[Trait("Category", "Integration")]
public class AreaDocenteQueryTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory() => new(ConnectionString);

    [Fact]
    public async Task ListarComisiones_DevuelveSoloLasDelDocenteConJoinsYOrdenCronologico()
    {
        var ct = CancellationToken.None;
        await using var cn = await Factory().CreateOpenConnectionAsync(ct);
        var (codProfes, esperadas) = await cn.QuerySingleAsync<(string, int)>("""
            SELECT FIRST 1 TRIM(CODPROFES), COUNT(*) FROM COMARM
            WHERE CODPROFES IS NOT NULL GROUP BY CODPROFES ORDER BY 2 DESC
            """);

        var comisiones = await new AreaDocenteQuery(Factory()).ListarComisionesAsync(codProfes, ct);

        Assert.Equal(esperadas, comisiones.Count);
        Assert.All(comisiones, c =>
        {
            Assert.False(string.IsNullOrWhiteSpace(c.CodigoCarrera));
            Assert.False(string.IsNullOrWhiteSpace(c.CodigoMateria));
            Assert.Equal(3, c.CuatrimestreAnio.Length);
            Assert.True(c.CantidadAlumnos >= 0);
        });
        // Sin precarga todavía: ningún estado.
        Assert.All(comisiones, c => Assert.Null(c.EstadoCarga));
        // Orden: año desc, luego cuatrimestre desc (sobre los códigos con patrón válido).
        var claves = comisiones.Select(c => c.CuatrimestreAnio)
            .Select(p => (Anio: p.Substring(1, 2), Cuat: p.Substring(0, 1))).ToList();
        Assert.Equal(claves.OrderByDescending(k => k.Anio).ThenByDescending(k => k.Cuat).ToList(), claves);
    }

    [Fact]
    public async Task ListarComisiones_DocenteInexistente_DevuelveVacio()
    {
        var comisiones = await new AreaDocenteQuery(Factory()).ListarComisionesAsync("ZZZ", CancellationToken.None);

        Assert.Empty(comisiones);
    }

    [Fact]
    public async Task ListarMesas_DevuelveSoloLasDelTitularYRecortaPorFecha()
    {
        var ct = CancellationToken.None;
        await using var cn = await Factory().CreateOpenConnectionAsync(ct);
        var (titular, esperadas, ultima) = await cn.QuerySingleAsync<(string, int, DateTime?)>("""
            SELECT FIRST 1 TRIM(TITULAR), COUNT(*), MAX(FECH_EXA) FROM MESAS
            WHERE TITULAR IS NOT NULL GROUP BY TITULAR ORDER BY 2 DESC
            """);

        var query = new AreaDocenteQuery(Factory());
        var todas = await query.ListarMesasAsync(titular, desde: null, ct);

        Assert.Equal(esperadas, todas.Count);
        Assert.All(todas, m => Assert.Null(m.EstadoCarga));
        Assert.Equal(todas.OrderByDescending(m => m.FechaExamen).Select(m => m.FechaExamen), todas.Select(m => m.FechaExamen));

        // Recorte: desde el día después de la última mesa no queda ninguna.
        Assert.NotNull(ultima);
        var posteriores = await query.ListarMesasAsync(titular, DateOnly.FromDateTime(ultima!.Value).AddDays(1), ct);
        Assert.Empty(posteriores);

        // Desde la fecha de la última mesa, al menos esa queda.
        var desdeUltima = await query.ListarMesasAsync(titular, DateOnly.FromDateTime(ultima.Value), ct);
        Assert.NotEmpty(desdeUltima);
        Assert.All(desdeUltima, m => Assert.True(m.FechaExamen >= DateOnly.FromDateTime(ultima.Value)));
    }
}
