using Dapper;
using Esba.Domain.Entities;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Examenes;

/// <summary>
/// Roundtrip del ABM de mesas contra Firebird real: wrapper XXX_VALIDO_MESA y
/// alta/baja por EF. Usa un número de mesa de prueba improbable y limpia siempre.
/// </summary>
[Trait("Category", "Integration")]
public class MesaRoundtripTests
{
    private const int MesaPrueba = 990001;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static async Task BorrarAsync(string carrera)
    {
        await using var ctx = CrearContexto();
        var conn = ctx.Database.GetDbConnection();
        await conn.ExecuteAsync("DELETE FROM MESAS WHERE CARRE=@C AND MESA=@M", new { C = carrera, M = MesaPrueba });
    }

    [Fact]
    public async Task ValidoMesa_MesaInexistente_DevuelveOk()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();
        var carrera = await ctx.Materias.AsNoTracking().Select(m => m.CodigoCarrera).FirstOrDefaultAsync(ct);
        if (carrera is null)
        {
            return;
        }

        await BorrarAsync(carrera);
        var resultado = await new ValidoMesaProcedure(Factory).VerificarAsync(MesaPrueba, carrera, ct);

        Assert.NotEqual(Esba.Domain.Common.OperationStatus.Error, resultado.Status);
    }

    [Fact]
    public async Task AltaYBaja_PersisteYLuegoBorra()
    {
        var ct = CancellationToken.None;
        await using var ctx0 = CrearContexto();
        var materia = await ctx0.Materias.AsNoTracking()
            .OrderBy(m => m.CodigoCarrera).ThenBy(m => m.Codigo).FirstOrDefaultAsync(ct);
        if (materia is null)
        {
            return;
        }

        var carrera = materia.CodigoCarrera;
        await BorrarAsync(carrera);

        try
        {
            await using (var ctx = CrearContexto())
            {
                var repo = new MesaRepository(ctx);
                var uow = new EfUnitOfWork(ctx);
                repo.Agregar(new Mesa
                {
                    CodigoCarrera = carrera,
                    NumeroMesa = MesaPrueba,
                    CodigoMateria = materia.Codigo,
                    FechaExamen = new DateOnly(2099, 7, 1),
                    Llamado = 1,
                    CodigoTipo = "01",
                    Usuario = "test",
                });
                await uow.SaveChangesAsync(ct);
            }

            // XXX_VALIDO_MESA ahora debe detectar el duplicado.
            var dup = await new ValidoMesaProcedure(Factory).VerificarAsync(MesaPrueba, carrera, ct);
            Assert.Equal(Esba.Domain.Common.OperationStatus.Error, dup.Status);

            await using (var verificacion = CrearContexto())
            {
                var existe = await verificacion.Mesas.AsNoTracking()
                    .AnyAsync(m => m.CodigoCarrera == carrera && m.NumeroMesa == MesaPrueba, ct);
                Assert.True(existe);
            }
        }
        finally
        {
            await BorrarAsync(carrera);
        }
    }
}
