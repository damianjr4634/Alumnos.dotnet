using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Application.Features.AreaDocente;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using NSubstitute;

namespace Esba.Application.Tests.AreaDocente;

/// <summary>Efectivizar (marcar EFE tras procesar en secretaría): solo secretaría, desde borrador o finalizada, no dos veces.</summary>
public class EfectivizarCargaHandlersTests
{
    private readonly ICargaComisionDocenteRepository _comisiones = Substitute.For<ICargaComisionDocenteRepository>();
    private readonly ICargaMesaDocenteRepository _mesas = Substitute.For<ICargaMesaDocenteRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly ClaveComision Comision = new() { CodigoCarrera = "TER", Cutuco = 101, CodigoMateria = "01", CuatrimestreAnio = "226" };
    private static readonly ClaveMesa Mesa = new() { CodigoCarrera = "TER", NumeroMesa = 5001 };
    private static readonly ActorCargaDocente Secretaria = new() { CodigoUsuario = 1, EsSecretaria = true };
    private static readonly ActorCargaDocente Titular = new() { CodigoUsuario = 7, CodigoDocente = "017" };

    private EfectivizarCargaComisionHandler Comisiones() => new(_comisiones, _unitOfWork, TimeProvider.System);

    private EfectivizarCargaMesaHandler Mesas() => new(_mesas, _unitOfWork, TimeProvider.System);

    private static CargaComisionDocente CargaComision(EstadoCargaDocente estado) => new()
    {
        Id = 10, CodigoCarrera = "TER", Cutuco = 101, CodigoMateria = "01", CuatrimestreAnio = "226", CodigoDocente = "017", Estado = estado,
    };

    private static CargaMesaDocente CargaMesa(EstadoCargaDocente estado) => new()
    {
        Id = 20, CodigoCarrera = "TER", NumeroMesa = 5001, CodigoDocente = "017", Estado = estado,
    };

    [Theory]
    [InlineData(EstadoCargaDocente.Borrador)]
    [InlineData(EstadoCargaDocente.Finalizada)]
    public async Task Comision_Secretaria_MarcaEfectivizadaYCommitea(EstadoCargaDocente desde)
    {
        var carga = CargaComision(desde);
        _comisiones.ObtenerPorComisionAsync(Comision, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Comisiones().HandleAsync(new CambiarEstadoCargaComisionCommand { Comision = Comision, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Efectivizada, carga.Estado);
        Assert.Equal(1, carga.CodigoUsuarioEfectivizacion);
        Assert.NotNull(carga.FechaEfectivizacion);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Comision_YaEfectivizada_DevuelveErrorSinCommit()
    {
        _comisiones.ObtenerPorComisionAsync(Comision, Arg.Any<CancellationToken>()).Returns(CargaComision(EstadoCargaDocente.Efectivizada));

        var resultado = await Comisiones().HandleAsync(new CambiarEstadoCargaComisionCommand { Comision = Comision, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Comision_DocenteTitular_DevuelveErrorSinCommit()
    {
        var carga = CargaComision(EstadoCargaDocente.Finalizada);
        _comisiones.ObtenerPorComisionAsync(Comision, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Comisiones().HandleAsync(new CambiarEstadoCargaComisionCommand { Comision = Comision, Actor = Titular }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Finalizada, carga.Estado);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Comision_SinCarga_DevuelveError()
    {
        _comisiones.ObtenerPorComisionAsync(Comision, Arg.Any<CancellationToken>()).Returns((CargaComisionDocente?)null);

        var resultado = await Comisiones().HandleAsync(new CambiarEstadoCargaComisionCommand { Comision = Comision, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }

    [Fact]
    public async Task Mesa_Secretaria_MarcaEfectivizadaYCommitea()
    {
        var carga = CargaMesa(EstadoCargaDocente.Finalizada);
        _mesas.ObtenerPorMesaAsync(Mesa, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Mesas().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Mesa, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Efectivizada, carga.Estado);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Mesa_DocenteTitular_DevuelveErrorSinCommit()
    {
        var carga = CargaMesa(EstadoCargaDocente.Finalizada);
        _mesas.ObtenerPorMesaAsync(Mesa, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Mesas().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Mesa, Actor = Titular }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Mesa_YaEfectivizada_DevuelveError()
    {
        _mesas.ObtenerPorMesaAsync(Mesa, Arg.Any<CancellationToken>()).Returns(CargaMesa(EstadoCargaDocente.Efectivizada));

        var resultado = await Mesas().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Mesa, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }
}
