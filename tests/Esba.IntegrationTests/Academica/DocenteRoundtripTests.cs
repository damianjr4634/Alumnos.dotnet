using Dapper;
using Esba.Application.DTOs.Academica;
using Esba.Application.Validators;
using Esba.Application.Features.Administracion;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Academica;

/// <summary>
/// Roundtrip del ABM de docentes contra Firebird real: ejercita el mapeo EF de
/// las columnas nuevas (CHAR con relleno, LICENCIA 'S'/'N', fechas) que los tests
/// unitarios con mocks no cubren. Usa un código improbable y limpia siempre.
/// </summary>
[Trait("Category", "Integration")]
public class DocenteRoundtripTests
{
    private const string CodigoPrueba = "Z99";

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static FbConnectionFactory Factory() => new(ConnectionString);

    private static async Task BorrarAsync()
    {
        await using var ctx = CrearContexto();
        await ctx.Database.GetDbConnection().ExecuteAsync(
            "DELETE FROM DOCENTES WHERE CODPROFES = @Cod", new { Cod = CodigoPrueba });
    }

    [Fact]
    public async Task Alta_Detalle_Baja_PersistenYSeLeenLasColumnasNuevas()
    {
        var ct = CancellationToken.None;
        await BorrarAsync();

        try
        {
            // Alta vía handler (EF).
            await using (var ctx = CrearContexto())
            {
                var handler = new CrearDocenteHandler(
                    new DocenteRepository(ctx), new CrearDocenteValidator(), new EfUnitOfWork(ctx));

                var alta = await handler.HandleAsync(new CrearDocenteCommand
                {
                    Codigo = CodigoPrueba,
                    Nombre = "Test, Roundtrip",
                    TipoDocumento = "DNI",
                    NumeroDocumento = "99999999",
                    Localidad = "CABA",
                    TelefonoParticular = "1144445555",
                    EnLicencia = true,
                    FechaIngreso = new DateOnly(2020, 3, 1),
                    FechaLicencia = new DateOnly(2026, 5, 10),
                }, ct);

                Assert.Equal(OperationStatus.Ok, alta.Status);
            }

            // Lectura del detalle (Dapper, con TRIM y conversión de LICENCIA).
            var detalle = await new DocentesQuery(Factory()).ObtenerDetalleAsync(CodigoPrueba, ct);
            Assert.NotNull(detalle);
            Assert.Equal("Test, Roundtrip", detalle!.Nombre);
            Assert.Equal("DNI", detalle.TipoDocumento);          // CHAR(3) sin relleno
            Assert.Equal("CABA", detalle.Localidad);             // CHAR(30) sin relleno
            Assert.True(detalle.EnLicencia);                     // LICENCIA 'S' → true
            Assert.Equal(new DateOnly(2020, 3, 1), detalle.FechaIngreso);
            Assert.Null(detalle.FechaBaja);

            // Baja lógica vía handler.
            await using (var ctx = CrearContexto())
            {
                var baja = await new DarDeBajaDocenteHandler(
                    new DocenteRepository(ctx), new EfUnitOfWork(ctx), TimeProvider.System)
                    .HandleAsync(CodigoPrueba, ct);
                Assert.Equal(OperationStatus.Ok, baja.Status);
            }

            var trasBaja = await new DocentesQuery(Factory()).ObtenerDetalleAsync(CodigoPrueba, ct);
            Assert.NotNull(trasBaja!.FechaBaja);
        }
        finally
        {
            await BorrarAsync();
        }
    }
}
