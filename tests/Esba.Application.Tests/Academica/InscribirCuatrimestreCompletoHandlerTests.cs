using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using Esba.Domain.Common;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class InscribirCuatrimestreCompletoHandlerTests
{
    private readonly IInscripcionMasivaCuatrimestreProcedure _procedimiento =
        Substitute.For<IInscripcionMasivaCuatrimestreProcedure>();

    private readonly ICarrerasQuery _carreras = Substitute.For<ICarrerasQuery>();

    private InscribirCuatrimestreCompletoHandler CrearHandler() =>
        new(_procedimiento, _carreras, new InscribirCuatrimestreCompletoValidator());

    private static InscribirCuatrimestreCompletoCommand ComandoValido() => new()
    {
        CodigoCarrera = "ADM",
        CodigoAlumno = "DNI30123456",
        Curso = 111,
        CuatrimestreAnio = "124",
        CodigoUsuario = 1,
    };

    [Fact]
    public async Task Previsualizar_EjecutaSinConfirmar()
    {
        _procedimiento.EjecutarAsync(Arg.Any<InscripcionMasivaParametros>(), false, Arg.Any<CancellationToken>())
            .Returns(Result.Warning("ok", "Se inscribió: ..."));

        var resultado = await CrearHandler().PrevisualizarAsync(ComandoValido(), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        await _procedimiento.Received(1).EjecutarAsync(Arg.Any<InscripcionMasivaParametros>(), false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirmar_EjecutaConConfirmar()
    {
        _procedimiento.EjecutarAsync(Arg.Any<InscripcionMasivaParametros>(), true, Arg.Any<CancellationToken>())
            .Returns(Result.Ok("ok"));

        await CrearHandler().ConfirmarAsync(ComandoValido(), CancellationToken.None);

        await _procedimiento.Received(1).EjecutarAsync(Arg.Any<InscripcionMasivaParametros>(), true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ejecutar_ResuelveInstitutoYCaracteristicaDeLaCarrera()
    {
        _carreras.ObtenerDatosInscripcionAsync("ADM", Arg.Any<CancellationToken>())
            .Returns(("Instituto X", "CA"));
        InscripcionMasivaParametros? capturados = null;
        _procedimiento.EjecutarAsync(Arg.Do<InscripcionMasivaParametros>(p => capturados = p), false, Arg.Any<CancellationToken>())
            .Returns(Result.Ok("ok"));

        await CrearHandler().PrevisualizarAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal("Instituto X", capturados!.Instituto);
        Assert.Equal("CA", capturados.Caracteristica);
        Assert.Equal("DNI30123456", capturados.CodigoAlumno);
        Assert.Equal((short)111, capturados.Curso);
    }

    [Fact]
    public async Task Ejecutar_ComandoInvalido_NoLlamaAlProcedimiento()
    {
        var resultado = await CrearHandler().PrevisualizarAsync(
            ComandoValido() with { Curso = 5 }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _procedimiento.DidNotReceive().EjecutarAsync(
            Arg.Any<InscripcionMasivaParametros>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
}
