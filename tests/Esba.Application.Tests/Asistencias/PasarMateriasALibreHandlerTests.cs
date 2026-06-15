using Esba.Application.Abstractions;
using Esba.Application.Features.Asistencias;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Asistencias;

public class PasarMateriasALibreHandlerTests
{
    private readonly IPaseLibreProcedure _procedimiento = Substitute.For<IPaseLibreProcedure>();

    private PasarMateriasALibreHandler CrearHandler() => new(_procedimiento);

    [Fact]
    public async Task Previsualizar_EjecutaSinConfirmar()
    {
        _procedimiento.EjecutarAsync("A1", "ADM", false, Arg.Any<CancellationToken>())
            .Returns(Result.NeedsConfirmation<string>("¿Seguro?"));

        var resultado = await CrearHandler().PrevisualizarAsync("A1", "ADM", CancellationToken.None);

        Assert.Equal(OperationStatus.NeedsConfirmation, resultado.Status);
        await _procedimiento.Received(1).EjecutarAsync("A1", "ADM", false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirmar_EjecutaConConfirmar()
    {
        _procedimiento.EjecutarAsync("A1", "ADM", true, Arg.Any<CancellationToken>())
            .Returns(Result.Ok("ok"));

        await CrearHandler().ConfirmarAsync("A1", "ADM", CancellationToken.None);

        await _procedimiento.Received(1).EjecutarAsync("A1", "ADM", true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ejecutar_DatosVacios_DevuelveErrorSinLlamarAlProcedimiento()
    {
        var resultado = await CrearHandler().ConfirmarAsync("", "ADM", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _procedimiento.DidNotReceive().EjecutarAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
}
