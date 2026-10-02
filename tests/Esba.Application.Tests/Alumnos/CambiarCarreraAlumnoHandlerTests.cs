using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Application.Features.Alumnos;
using Esba.Application.Validators;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Alumnos;

public class CambiarCarreraAlumnoHandlerTests
{
    private readonly ICopiaAlumnoProcedure _copia = Substitute.For<ICopiaAlumnoProcedure>();
    private readonly IMueveAlumnoProcedure _mueve = Substitute.For<IMueveAlumnoProcedure>();

    private CambiarCarreraAlumnoHandler CrearHandler() => new(new CambiarCarreraAlumnoValidator(), _copia, _mueve);

    private static CambiarCarreraAlumnoCommand Comando(
        string alumno = "DNI12345678", string origen = "TEC", string destino = "BAC", int usuario = 7) => new()
    {
        CodigoAlumno = alumno,
        CodigoCarreraOrigen = origen,
        CodigoCarreraDestino = destino,
        CodigoUsuario = usuario,
    };

    [Fact]
    public async Task Copia_SinDestino_DevuelveError_YNoLlamaAlSp()
    {
        var resultado = await CrearHandler().PrevisualizarCopiaAsync(Comando(destino: ""), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("Elegí la carrera de destino.", resultado.Message);
        await _copia.DidNotReceiveWithAnyArgs().EjecutarAsync(default!, default, default);
    }

    [Fact]
    public async Task Copia_DestinoIgualAlOrigen_DevuelveError()
    {
        var resultado = await CrearHandler().PrevisualizarCopiaAsync(Comando(destino: " tec "), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("La carrera de destino debe ser distinta de la de origen.", resultado.Message);
    }

    [Fact]
    public async Task Copia_UsuarioInvalido_DevuelveError()
    {
        var resultado = await CrearHandler().ConfirmarCopiaAsync(Comando(usuario: 0), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("Usuario inválido.", resultado.Message);
        await _copia.DidNotReceiveWithAnyArgs().EjecutarAsync(default!, default, default);
    }

    [Fact]
    public async Task Copia_Previsualizar_LlamaAlSpSinConfirmar_YRecortaParametros()
    {
        CambioCarreraAlumnoParametros? capturado = null;
        _copia.EjecutarAsync(Arg.Do<CambioCarreraAlumnoParametros>(p => capturado = p), false, Arg.Any<CancellationToken>())
            .Returns(Result.NeedsConfirmation<string>("El Alumno X fue copiado a la carrera BAC, Confirma?"));

        var resultado = await CrearHandler().PrevisualizarCopiaAsync(
            Comando(alumno: " DNI12345678 ", origen: " TEC", destino: "BAC "), CancellationToken.None);

        Assert.Equal(OperationStatus.NeedsConfirmation, resultado.Status);
        Assert.NotNull(capturado);
        Assert.Equal("DNI12345678", capturado!.CodigoAlumno);
        Assert.Equal("TEC", capturado.CodigoCarreraOrigen);
        Assert.Equal("BAC", capturado.CodigoCarreraDestino);
        Assert.Equal(7, capturado.CodigoUsuario);
        await _mueve.DidNotReceiveWithAnyArgs().EjecutarAsync(default!, default, default);
    }

    [Fact]
    public async Task Copia_Confirmar_LlamaAlSpConfirmando()
    {
        _copia.EjecutarAsync(Arg.Any<CambioCarreraAlumnoParametros>(), true, Arg.Any<CancellationToken>())
            .Returns(Result.Ok("Alumno copiado a la carrera BAC."));

        var resultado = await CrearHandler().ConfirmarCopiaAsync(Comando(), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("Alumno copiado a la carrera BAC.", resultado.Value);
    }

    [Fact]
    public async Task Mover_Previsualizar_LlamaAlSpDeMover_YPropagaElError()
    {
        _mueve.EjecutarAsync(Arg.Any<CambioCarreraAlumnoParametros>(), false, Arg.Any<CancellationToken>())
            .Returns(Result.Error<string>("El alumno tiene materias cargadas. No se puede mover al alumno"));

        var resultado = await CrearHandler().PrevisualizarMovimientoAsync(Comando(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("materias cargadas", resultado.Message, StringComparison.Ordinal);
        await _copia.DidNotReceiveWithAnyArgs().EjecutarAsync(default!, default, default);
    }

    [Fact]
    public async Task Mover_Confirmar_PropagaElCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        _mueve.EjecutarAsync(Arg.Any<CambioCarreraAlumnoParametros>(), true, cts.Token)
            .Returns(Result.Ok("Alumno movido a la carrera BAC."));

        var resultado = await CrearHandler().ConfirmarMovimientoAsync(Comando(), cts.Token);

        Assert.True(resultado.IsSuccess);
        await _mueve.Received(1).EjecutarAsync(Arg.Any<CambioCarreraAlumnoParametros>(), true, cts.Token);
    }
}
