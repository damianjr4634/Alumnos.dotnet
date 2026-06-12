using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Application.Features.Alumnos;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using NSubstitute;

namespace Esba.Application.Tests.Alumnos;

public class CrearAlumnoHandlerTests
{
    private readonly IAlumnoRepository _alumnos = Substitute.For<IAlumnoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CrearAlumnoHandler CrearHandler() =>
        new(_alumnos, new CrearAlumnoValidator(), _unitOfWork);

    private static CrearAlumnoCommand ComandoValido() => new()
    {
        TipoDocumento = "DNI",
        NumeroDocumento = "30123456",
        CodigoCarrera = "ADM",
        Apellido = "García",
        Nombre = "Ana",
        Estado = EstadoAlumno.Cursando,
    };

    [Fact]
    public async Task Crear_AlumnoNuevo_DevuelveOkConCodigoYCommiteaUnaVez()
    {
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>())
            .Returns((Alumno?)null);
        Alumno? agregado = null;
        _alumnos.When(a => a.Agregar(Arg.Any<Alumno>())).Do(ci => agregado = ci.Arg<Alumno>());

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("DNI30123456", resultado.Value);
        Assert.NotNull(agregado);
        Assert.False(agregado.Baja);
        Assert.Equal("damian", agregado.Usuario);
        Assert.Equal("García", agregado.Apellido);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_DuplicadoActivo_DevuelveErrorEnAltasYNoCommitea()
    {
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>())
            .Returns(new Alumno { Codigo = "DNI30123456", CodigoCarrera = "ADM", Baja = false });

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("altas", resultado.Message, StringComparison.OrdinalIgnoreCase);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Crear_DuplicadoDeBaja_DevuelveErrorEnBajasConNombre()
    {
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>())
            .Returns(new Alumno
            {
                Codigo = "DNI30123456",
                CodigoCarrera = "ADM",
                Baja = true,
                Apellido = "Pérez",
                Nombre = "Juan",
            });

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("bajas", resultado.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Pérez", resultado.Message, StringComparison.Ordinal);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Crear_ComandoInvalido_DevuelveErrorSinConsultarRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoValido() with { NumeroDocumento = "" }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _alumnos.DidNotReceiveWithAnyArgs().ObtenerAsync(default!, default!, default);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Crear_NumeroCorto_GeneraCodigoConCerosALaIzquierda()
    {
        _alumnos.ObtenerAsync("ADM", "DNI01234567", Arg.Any<CancellationToken>())
            .Returns((Alumno?)null);

        var resultado = await CrearHandler().HandleAsync(
            ComandoValido() with { NumeroDocumento = "1234567" }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("DNI01234567", resultado.Value);
    }
}
