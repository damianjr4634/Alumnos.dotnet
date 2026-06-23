using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Administracion;

public class DocenteHandlersTests
{
    private readonly IDocenteRepository _docentes = Substitute.For<IDocenteRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CrearDocenteHandler CrearHandler() =>
        new(_docentes, new CrearDocenteValidator(), _unitOfWork);

    private ActualizarDocenteHandler ActualizarHandler() =>
        new(_docentes, new ActualizarDocenteValidator(), _unitOfWork);

    private DarDeBajaDocenteHandler BajaHandler() =>
        new(_docentes, _unitOfWork, TimeProvider.System);

    private ReactivarDocenteHandler ReactivarHandler() =>
        new(_docentes, _unitOfWork);

    private static CrearDocenteCommand Alta() => new()
    {
        Codigo = "012",
        Nombre = "Pérez, Juan",
        TipoDocumento = "DNI",
        NumeroDocumento = "12345678",
        Localidad = "CABA",
    };

    private static Docente Activo(string codigo = "012") => new()
    {
        Codigo = codigo,
        Nombre = "Pérez, Juan",
        FechaBaja = null,
    };

    [Fact]
    public async Task Crear_DocenteNuevo_AgregaYCommiteaUnaVez()
    {
        _docentes.ExisteAsync("012", Arg.Any<CancellationToken>()).Returns(false);
        Docente? capturado = null;
        _docentes.When(d => d.Agregar(Arg.Any<Docente>())).Do(ci => capturado = ci.Arg<Docente>());

        var resultado = await CrearHandler().HandleAsync(Alta(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _docentes.Received(1).Agregar(Arg.Any<Docente>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("012", capturado!.Codigo);
        Assert.Equal("CABA", capturado.Localidad);
    }

    [Fact]
    public async Task Crear_CodigoDuplicado_DevuelveErrorSinCommit()
    {
        _docentes.ExisteAsync("012", Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await CrearHandler().HandleAsync(Alta(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _docentes.DidNotReceive().Agregar(Arg.Any<Docente>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_ComandoInvalido_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(Alta() with { Nombre = "" }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _docentes.DidNotReceive().ExisteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_DocenteExistente_AplicaCambiosYCommitea()
    {
        var docente = Activo();
        _docentes.ObtenerPorCodigoAsync("012", Arg.Any<CancellationToken>()).Returns(docente);

        var resultado = await ActualizarHandler().HandleAsync(new ActualizarDocenteCommand
        {
            Codigo = "012",
            Nombre = "Gómez, Ana",
            Localidad = "Quilmes",
            EnLicencia = true,
        }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("Gómez, Ana", docente.Nombre);
        Assert.True(docente.EnLicencia);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_DocenteInexistente_DevuelveErrorSinCommit()
    {
        _docentes.ObtenerPorCodigoAsync("012", Arg.Any<CancellationToken>()).Returns((Docente?)null);

        var resultado = await ActualizarHandler().HandleAsync(new ActualizarDocenteCommand
        {
            Codigo = "012",
            Nombre = "Gómez, Ana",
        }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DarDeBaja_DocenteActivo_SeteaFechaYCommitea()
    {
        var docente = Activo();
        _docentes.ObtenerPorCodigoAsync("012", Arg.Any<CancellationToken>()).Returns(docente);

        var resultado = await BajaHandler().HandleAsync("012", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(docente.FechaBaja);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DarDeBaja_YaDeBaja_DevuelveWarningSinCommit()
    {
        var docente = Activo();
        docente.FechaBaja = new DateOnly(2026, 1, 1);
        _docentes.ObtenerPorCodigoAsync("012", Arg.Any<CancellationToken>()).Returns(docente);

        var resultado = await BajaHandler().HandleAsync("012", CancellationToken.None);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DarDeBaja_Inexistente_DevuelveError()
    {
        _docentes.ObtenerPorCodigoAsync("012", Arg.Any<CancellationToken>()).Returns((Docente?)null);

        var resultado = await BajaHandler().HandleAsync("012", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivar_DocenteDeBaja_LimpiaFechaYCommitea()
    {
        var docente = Activo();
        docente.FechaBaja = new DateOnly(2026, 1, 1);
        _docentes.ObtenerPorCodigoAsync("012", Arg.Any<CancellationToken>()).Returns(docente);

        var resultado = await ReactivarHandler().HandleAsync("012", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Null(docente.FechaBaja);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reactivar_DocenteActivo_DevuelveWarningSinCommit()
    {
        _docentes.ObtenerPorCodigoAsync("012", Arg.Any<CancellationToken>()).Returns(Activo());

        var resultado = await ReactivarHandler().HandleAsync("012", CancellationToken.None);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
