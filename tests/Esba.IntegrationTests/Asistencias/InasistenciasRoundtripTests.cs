using Dapper;
using Esba.Application.DTOs.Asistencias;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Asistencias;

/// <summary>
/// Roundtrip de la escritura de inasistencias contra Firebird real. Usa un año
/// (2099) y un CUTUCO de prueba para no tocar datos reales (el delete del
/// reemplazo está acotado por carrera+cutuco+año) y limpia siempre.
/// </summary>
[Trait("Category", "Integration")]
public class InasistenciasRoundtripTests
{
    private const short CutucoPrueba = 999;
    private const int AnioPrueba = 2099;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static async Task<int> ContarAsync(string carrera)
    {
        await using var ctx = CrearContexto();
        var conn = ctx.Database.GetDbConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM FALTAS WHERE CARRERA=@C AND CUTUCO=@Cu AND EXTRACT(YEAR FROM FECHA)=@A",
            new { C = carrera, Cu = (int)CutucoPrueba, A = AnioPrueba });
    }

    private static async Task LimpiarAsync(string carrera)
    {
        await using var ctx = CrearContexto();
        var conn = ctx.Database.GetDbConnection();
        await conn.ExecuteAsync(
            "DELETE FROM FALTAS WHERE CARRERA=@C AND CUTUCO=@Cu AND EXTRACT(YEAR FROM FECHA)=@A",
            new { C = carrera, Cu = (int)CutucoPrueba, A = AnioPrueba });
    }

    [Fact]
    public async Task Reemplazar_InsertaYLuegoBorra()
    {
        var ct = CancellationToken.None;

        await using var ctx = CrearContexto();
        var carrera = await ctx.Materias.AsNoTracking().Select(m => m.CodigoCarrera).FirstOrDefaultAsync(ct);
        var tipo = await ctx.TiposFalta.AsNoTracking().Select(t => t.Codigo).FirstOrDefaultAsync(ct);
        if (carrera is null || tipo is null)
        {
            return; // base sin materias/tipos; nada que verificar.
        }

        await LimpiarAsync(carrera);
        var repo = new InasistenciasRepository(Factory);

        try
        {
            var faltas = new List<FaltaInasistencia>
            {
                new() { CodigoAlumno = "TESTALU0001", Fecha = new DateOnly(AnioPrueba, 3, 15), CodigoFalta = tipo, Cantidad = 1 },
                new() { CodigoAlumno = "TESTALU0001", Fecha = new DateOnly(AnioPrueba, 3, 22), CodigoFalta = tipo, Cantidad = 1 },
            };

            var insertados = await repo.ReemplazarFaltasComisionAsync(carrera, CutucoPrueba, null, AnioPrueba, 1, faltas, ct);

            Assert.Equal(2, insertados);
            Assert.Equal(2, await ContarAsync(carrera));

            // Reemplazo con lista vacía ⇒ borra todo lo del año.
            await repo.ReemplazarFaltasComisionAsync(carrera, CutucoPrueba, null, AnioPrueba, 1, [], ct);
            Assert.Equal(0, await ContarAsync(carrera));
        }
        finally
        {
            await LimpiarAsync(carrera);
        }
    }
}
