using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class CicloLectivoHandlersTests
{
    private readonly ICicloLectivoRepository _ciclos = Substitute.For<ICicloLectivoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private GuardarCicloCuatrimestralHandler CuatrimestralHandler() =>
        new(_ciclos, new GuardarCicloCuatrimestralValidator(), _unitOfWork);

    private GuardarCicloTrimestralHandler TrimestralHandler() =>
        new(_ciclos, new GuardarCicloTrimestralValidator(), _unitOfWork);

    private EliminarCicloLectivoHandler EliminarHandler() => new(_ciclos, _unitOfWork);

    private static GuardarCicloCuatrimestralCommand Alta(bool esNuevo = true) => new()
    {
        EsNuevo = esNuevo,
        Anio = 2026,
        PrimerCuatrimestreDesde = new DateOnly(2026, 3, 2),
        PrimerCuatrimestreHasta = new DateOnly(2026, 7, 10),
        SegundoCuatrimestreDesde = new DateOnly(2026, 8, 3),
        SegundoCuatrimestreHasta = new DateOnly(2026, 12, 4),
    };

    private static GuardarCicloTrimestralCommand AltaTrimestral(bool esNuevo = true) => new()
    {
        EsNuevo = esNuevo,
        Anio = 2026,
        PrimerTrimestreDesde = new DateOnly(2026, 3, 2),
        PrimerTrimestreHasta = new DateOnly(2026, 5, 29),
        SegundoTrimestreDesde = new DateOnly(2026, 6, 1),
        SegundoTrimestreHasta = new DateOnly(2026, 9, 4),
        TercerTrimestreDesde = new DateOnly(2026, 9, 7),
        TercerTrimestreHasta = new DateOnly(2026, 12, 4),
    };

    [Fact]
    public async Task Guardar_AnioNuevo_AgregaYCommiteaUnaVez()
    {
        _ciclos.ObtenerCuatrimestralAsync(2026, Arg.Any<CancellationToken>())
            .Returns((CicloCuatrimestral?)null);
        CicloCuatrimestral? capturado = null;
        _ciclos.When(c => c.Agregar(Arg.Any<CicloCuatrimestral>()))
            .Do(ci => capturado = ci.Arg<CicloCuatrimestral>());

        var resultado = await CuatrimestralHandler().HandleAsync(Alta(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _ciclos.Received(1).Agregar(Arg.Any<CicloCuatrimestral>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(2026, capturado!.Anio);
        Assert.Equal(new DateOnly(2026, 12, 4), capturado.SegundoCuatrimestreHasta);
    }

    [Fact]
    public async Task Guardar_AltaDeAnioExistente_DevuelveErrorSinCommit()
    {
        _ciclos.ObtenerCuatrimestralAsync(2026, Arg.Any<CancellationToken>())
            .Returns(new CicloCuatrimestral { Anio = 2026 });

        var resultado = await CuatrimestralHandler().HandleAsync(Alta(), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_EdicionDeAnioInexistente_DevuelveErrorSinCommit()
    {
        _ciclos.ObtenerCuatrimestralAsync(2026, Arg.Any<CancellationToken>())
            .Returns((CicloCuatrimestral?)null);

        var resultado = await CuatrimestralHandler().HandleAsync(Alta(esNuevo: false), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_Edicion_ActualizaLaEntidadTrackeadaSinAgregar()
    {
        var existente = new CicloCuatrimestral
        {
            Anio = 2026,
            PrimerCuatrimestreDesde = new DateOnly(2026, 3, 9),
        };
        _ciclos.ObtenerCuatrimestralAsync(2026, Arg.Any<CancellationToken>()).Returns(existente);

        var resultado = await CuatrimestralHandler().HandleAsync(Alta(esNuevo: false), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _ciclos.DidNotReceive().Agregar(Arg.Any<CicloCuatrimestral>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(new DateOnly(2026, 3, 2), existente.PrimerCuatrimestreDesde);
    }

    [Fact]
    public async Task Guardar_ComandoInvalido_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await CuatrimestralHandler().HandleAsync(
            Alta() with { PrimerCuatrimestreDesde = null }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _ciclos.DidNotReceive().ObtenerCuatrimestralAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GuardarTrimestral_AnioNuevo_AgregaConLosTresTrimestres()
    {
        _ciclos.ObtenerTrimestralAsync(2026, Arg.Any<CancellationToken>())
            .Returns((CicloTrimestral?)null);
        CicloTrimestral? capturado = null;
        _ciclos.When(c => c.Agregar(Arg.Any<CicloTrimestral>()))
            .Do(ci => capturado = ci.Arg<CicloTrimestral>());

        var resultado = await TrimestralHandler().HandleAsync(AltaTrimestral(), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(new DateOnly(2026, 9, 7), capturado!.TercerTrimestreDesde);
    }

    [Fact]
    public async Task Eliminar_AnioCuatrimestralExistente_EliminaYCommitea()
    {
        var existente = new CicloCuatrimestral { Anio = 2026 };
        _ciclos.ObtenerCuatrimestralAsync(2026, Arg.Any<CancellationToken>()).Returns(existente);

        var resultado = await EliminarHandler().HandleAsync(
            new EliminarCicloLectivoCommand { Anio = 2026, Trimestral = false }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _ciclos.Received(1).Eliminar(existente);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Eliminar_AnioTrimestralInexistente_DevuelveErrorSinCommit()
    {
        _ciclos.ObtenerTrimestralAsync(2026, Arg.Any<CancellationToken>())
            .Returns((CicloTrimestral?)null);

        var resultado = await EliminarHandler().HandleAsync(
            new EliminarCicloLectivoCommand { Anio = 2026, Trimestral = true }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
