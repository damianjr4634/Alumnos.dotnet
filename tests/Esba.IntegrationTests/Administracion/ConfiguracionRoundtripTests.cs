using Dapper;
using Esba.Application.DTOs.Administracion;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Administracion;

/// <summary>
/// Roundtrip de la configuración del sistema (XXX_CONF) contra Firebird real:
/// inserta un parámetro de prueba, lo edita vía handler (EF) y lo relee vía query
/// (Dapper). Usa un PARAME improbable y limpia siempre.
/// </summary>
[Trait("Category", "Integration")]
public class ConfiguracionRoundtripTests
{
    private const string ParamePrueba = "ZZZ_TEST_103A";

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
            "DELETE FROM XXX_CONF WHERE PARAME = @P", new { P = ParamePrueba });
    }

    [Fact]
    public async Task EditarValor_PersisteYSeReleeElNuevoValor()
    {
        var ct = CancellationToken.None;
        await BorrarAsync();

        try
        {
            // Sembrar el parámetro con un valor inicial.
            await using (var ctx = CrearContexto())
            {
                await ctx.Database.GetDbConnection().ExecuteAsync(
                    "INSERT INTO XXX_CONF (PARAME, DESCRI, VALOR) VALUES (@P, @D, @V)",
                    new { P = ParamePrueba, D = "Parametro de prueba 10.3a", V = "inicial" });
            }

            // Editar el VALOR vía handler (EF).
            await using (var ctx = CrearContexto())
            {
                var handler = new ActualizarConfiguracionHandler(
                    new ConfiguracionRepository(ctx),
                    new ActualizarConfiguracionValidator(),
                    new EfUnitOfWork(ctx));

                var resultado = await handler.HandleAsync(new ActualizarConfiguracionCommand
                {
                    Valores = [new ValorParametro { Parame = ParamePrueba, Valor = "modificado" }],
                }, ct);

                Assert.Equal(OperationStatus.Ok, resultado.Status);
                Assert.Equal(1, resultado.Value);
            }

            // Releer vía query (Dapper) y verificar el nuevo valor.
            var lista = await new ConfiguracionQuery(Factory()).ListarAsync(ct);
            var parametro = lista.SingleOrDefault(p => p.Parame == ParamePrueba);
            Assert.NotNull(parametro);
            Assert.Equal("modificado", parametro!.Valor);
            Assert.Equal("Parametro de prueba 10.3a", parametro.Descripcion);
        }
        finally
        {
            await BorrarAsync();
        }
    }
}
