using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Application.Features.Alumnos;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using NSubstitute;

namespace Esba.Application.Tests.Alumnos;

public class ActualizarAlumnoHandlerTests
{
    private readonly IAlumnoRepository _alumnos = Substitute.For<IAlumnoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ActualizarAlumnoHandler CrearHandler() =>
        new(_alumnos, new ActualizarAlumnoValidator(), _unitOfWork);

    private static ActualizarAlumnoCommand ComandoValido() => new()
    {
        Codigo = "DNI30123456",
        CodigoCarrera = "ADM",
        Apellido = "García",
        Nombre = "Ana María",
        Estado = EstadoAlumno.Egresado,
        Baja = false,
    };

    private static Alumno AlumnoExistente() => new()
    {
        Codigo = "DNI30123456",
        CodigoCarrera = "ADM",
        Apellido = "García",
        Nombre = "Ana",
        Estado = EstadoAlumno.Cursando,
        Foto = [1, 2, 3],
    };

    [Fact]
    public async Task Actualizar_Existente_AplicaCambiosYCommiteaUnaVez()
    {
        var alumno = AlumnoExistente();
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>()).Returns(alumno);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("Ana María", alumno.Nombre);
        Assert.Equal(EstadoAlumno.Egresado, alumno.Estado);
        Assert.Equal("damian", alumno.Usuario);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_Inexistente_DevuelveErrorYNoCommitea()
    {
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>())
            .Returns((Alumno?)null);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("El alumno no existe.", resultado.Message);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Actualizar_SinFotoNueva_ConservaLaFotoExistente()
    {
        var alumno = AlumnoExistente();
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>()).Returns(alumno);

        await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal([1, 2, 3], alumno.Foto);
    }

    [Fact]
    public async Task Actualizar_MarcaBaja_AplicaLaBaja()
    {
        var alumno = AlumnoExistente();
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>()).Returns(alumno);

        var resultado = await CrearHandler().HandleAsync(
            ComandoValido() with { Baja = true }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.True(alumno.Baja);
    }

    [Fact]
    public async Task Actualizar_ComandoInvalido_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoValido() with { Apellido = "" }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _alumnos.DidNotReceiveWithAnyArgs().ObtenerAsync(default!, default!, default);
    }
}
