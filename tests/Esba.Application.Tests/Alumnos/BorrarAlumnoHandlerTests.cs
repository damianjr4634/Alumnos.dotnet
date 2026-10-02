using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Application.Features.Alumnos;
using Esba.Application.Validators;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Alumnos;

public class BorrarAlumnoHandlerTests
{
    private readonly IBorraAlumnoProcedure _borra = Substitute.For<IBorraAlumnoProcedure>();

    private BorrarAlumnoHandler CrearHandler() => new(new BorrarAlumnoValidator(), _borra);

    private static BorrarAlumnoCommand Comando(
        string alumno = "DNI12345678", string carrera = "TEC", int usuario = 7, bool supervisor = true) => new()
    {
        CodigoAlumno = alumno,
        CodigoCarrera = carrera,
        CodigoUsuario = usuario,
        EsSupervisor = supervisor,
    };

    [Fact]
    public async Task Previsualizar_SinCarrera_DevuelveError_YNoLlamaAlSp()
    {
        var resultado = await CrearHandler().PrevisualizarAsync(Comando(carrera: ""), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("La carrera es obligatoria.", resultado.Message);
        await _borra.DidNotReceiveWithAnyArgs().EjecutarAsync(default!, default!, default, default, default);
    }

    [Fact]
    public async Task Previsualizar_NoSupervisor_DeniegaEnElServidor_SinTocarLaBase()
    {
        var resultado = await CrearHandler().PrevisualizarAsync(Comando(supervisor: false), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(BorrarAlumnoHandler.MensajeSoloSupervisor, resultado.Message);
        await _borra.DidNotReceiveWithAnyArgs().EjecutarAsync(default!, default!, default, default, default);
    }

    [Fact]
    public async Task Confirmar_NoSupervisor_TambienSeDeniega()
    {
        var resultado = await CrearHandler().ConfirmarAsync(Comando(supervisor: false), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _borra.DidNotReceiveWithAnyArgs().EjecutarAsync(default!, default!, default, default, default);
    }

    [Fact]
    public async Task Previsualizar_Supervisor_LlamaAlSpSinConfirmar_YDevuelveLaAdvertencia()
    {
        _borra.EjecutarAsync("TEC", "DNI12345678", 7, false, Arg.Any<CancellationToken>())
            .Returns(Result.NeedsConfirmation<string>("Esta operacion borra a un alumno y todo su historial…"));

        var resultado = await CrearHandler().PrevisualizarAsync(
            Comando(alumno: " DNI12345678 ", carrera: "TEC "), CancellationToken.None);

        Assert.Equal(OperationStatus.NeedsConfirmation, resultado.Status);
        Assert.StartsWith("Esta operacion borra", resultado.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Confirmar_Supervisor_LlamaAlSpConfirmando()
    {
        using var cts = new CancellationTokenSource();
        _borra.EjecutarAsync("TEC", "DNI12345678", 7, true, cts.Token)
            .Returns(Result.Ok("Alumno eliminado con todo su historial."));

        var resultado = await CrearHandler().ConfirmarAsync(Comando(), cts.Token);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("Alumno eliminado con todo su historial.", resultado.Value);
        await _borra.Received(1).EjecutarAsync("TEC", "DNI12345678", 7, true, cts.Token);
    }
}
