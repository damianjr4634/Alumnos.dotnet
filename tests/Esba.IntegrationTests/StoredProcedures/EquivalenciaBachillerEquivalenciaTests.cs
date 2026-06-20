using Dapper;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia del wrapper de impresión de equivalencia bachiller (hito 9.3c) y de la
/// query de encabezado contra la ejecución directa en Firebird. Read-only: el SP escribe
/// en la GTT TMP_EQUI (ON COMMIT DELETE ROWS), que se limpia sola al commitear.
/// </summary>
[Trait("Category", "Integration")]
public class EquivalenciaBachillerEquivalenciaTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static async Task<(string Codigo, string Carrera)?> AlumnoConEquivalenciaAsync(CancellationToken ct)
    {
        await using var ctx = CrearContexto();
        var fila = await ctx.Database.GetDbConnection().QueryFirstOrDefaultAsync<(string Cod, string Carre)>(
            "SELECT FIRST 1 TRIM(COD_ALU), TRIM(CARRE) FROM ANALITIC WHERE CONDICION = 'EQUIVALENCIA' ORDER BY CARRE, COD_ALU");

        return fila.Cod is null ? null : (fila.Cod, fila.Carre);
    }

    [Fact]
    public async Task ImpresionEqBac_FilasCoincidenConSpDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoConEquivalenciaAsync(ct) is not { } alumno)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var directo = (await ctx.Database.GetDbConnection()
            .QueryAsync<(string? Columna1, string? Columna2)>(
                "SELECT COLUMNA1, COLUMNA2 FROM XXX_IMPRESION_EQ_BAC(@A, @C)",
                new { A = alumno.Codigo, C = alumno.Carrera })).ToList();

        var wrapper = await new ImpresionEquivalenciaBachillerProcedure(Factory)
            .ListarLineasAsync(alumno.Codigo, alumno.Carrera, ct);

        Assert.Equal(directo.Count, wrapper.Count);
        for (var i = 0; i < directo.Count; i++)
        {
            Assert.Equal(directo[i].Columna1, wrapper[i].Columna1);
            Assert.Equal(directo[i].Columna2, wrapper[i].Columna2);
        }
    }

    [Fact]
    public async Task EncabezadoEquivalenciaBachiller_TraeDatosYTipoDeLaCarrera()
    {
        var ct = CancellationToken.None;
        if (await AlumnoConEquivalenciaAsync(ct) is not { } alumno)
        {
            return;
        }

        await using var ctx = CrearContexto();
        var tipoDirecto = await ctx.Database.GetDbConnection().ExecuteScalarAsync<string?>(
            "SELECT TRIM(TIPO) FROM CARRERA WHERE TRIM(CARRE) = @C", new { C = alumno.Carrera });

        var encabezado = await new ConstanciasQuery(Factory)
            .ObtenerEncabezadoEquivalenciaBachillerAsync(alumno.Codigo, alumno.Carrera, ct);

        Assert.NotNull(encabezado);
        Assert.Equal(tipoDirecto, encabezado!.TipoCarrera);
        Assert.False(string.IsNullOrWhiteSpace(encabezado.Alumno));
    }
}
