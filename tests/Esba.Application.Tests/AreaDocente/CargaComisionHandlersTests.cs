using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Application.Features.AreaDocente;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using NSubstitute;

namespace Esba.Application.Tests.AreaDocente;

/// <summary>
/// Reglas de la precarga por comisión (hito 19, decisiones 2026-10-03): solo el titular
/// carga y solo en borrador; secretaría puede siempre; finalizar cierra; reabrir es de
/// secretaría. En error no hay commit; en éxito hay exactamente uno.
/// </summary>
public class CargaComisionHandlersTests
{
    private readonly ICargaComisionDocenteRepository _cargas = Substitute.For<ICargaComisionDocenteRepository>();
    private readonly ICargaComisionDocenteQuery _consulta = Substitute.For<ICargaComisionDocenteQuery>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static readonly ClaveComision Clave = new()
    {
        CodigoCarrera = "TER", Cutuco = 101, CodigoMateria = "01", CuatrimestreAnio = "226",
    };

    private static readonly ActorCargaDocente Titular = new() { CodigoUsuario = 7, CodigoDocente = "017" };
    private static readonly ActorCargaDocente OtroDocente = new() { CodigoUsuario = 8, CodigoDocente = "018" };
    private static readonly ActorCargaDocente Secretaria = new() { CodigoUsuario = 1, EsSecretaria = true };

    private GuardarCargaComisionHandler Guardar() =>
        new(_cargas, _consulta, new GuardarCargaComisionValidator(), _unitOfWork, TimeProvider.System);

    private FinalizarCargaComisionHandler Finalizar() => new(_cargas, _unitOfWork, TimeProvider.System);

    private ReabrirCargaComisionHandler Reabrir() => new(_cargas, _unitOfWork, TimeProvider.System);

    private static CargaComisionDocenteDto Comision(string? titular = "017") => new()
    {
        CodigoCarrera = "TER", Cutuco = 101, CodigoMateria = "01", CuatrimestreAnio = "226",
        CodigoDocenteTitular = titular,
        Alumnos =
        [
            new AlumnoCargaComisionDto { CodigoAlumno = "30111222", CursadaIndice = 500, Apellido = "Pérez" },
            new AlumnoCargaComisionDto { CodigoAlumno = "30333444", CursadaIndice = 501, Apellido = "Gómez" },
        ],
    };

    private static CargaComisionDocente CargaExistente(EstadoCargaDocente estado = EstadoCargaDocente.Borrador) => new()
    {
        Id = 10, CodigoCarrera = "TER", Cutuco = 101, CodigoMateria = "01", CuatrimestreAnio = "226",
        CodigoDocente = "017", Estado = estado,
        Detalles =
        [
            new CargaComisionDocenteDetalle { Id = 1, CargaId = 10, CodigoAlumno = "30111222   ", Evaluacion1 = 4m },
        ],
    };

    private static GuardarCargaComisionCommand Comando(ActorCargaDocente actor, params FilaCargaComisionInput[] filas) => new()
    {
        Comision = Clave, Actor = actor, Filas = filas,
    };

    [Fact]
    public async Task Guardar_PrimeraVezPorElTitular_CreaCabeceraYDetallesNoVaciosYCommitea()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Comision());
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaComisionDocente?)null);
        CargaComisionDocente? capturada = null;
        _cargas.When(c => c.Agregar(Arg.Any<CargaComisionDocente>())).Do(ci => capturada = ci.Arg<CargaComisionDocente>());

        var resultado = await Guardar().HandleAsync(Comando(Titular,
            new FilaCargaComisionInput { CodigoAlumno = "30111222", Evaluacion1 = 7m, TotalHoras = 64 },
            new FilaCargaComisionInput { CodigoAlumno = "30333444" }), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(capturada);
        Assert.Equal("017", capturada!.CodigoDocente);
        Assert.Equal(EstadoCargaDocente.Borrador, capturada.Estado);
        Assert.Equal(7, capturada.CodigoUsuarioAlta);
        // La fila vacía de Gómez no se crea; la de Pérez sí, con el INDICE de CURSADA.
        var detalle = Assert.Single(capturada.Detalles);
        Assert.Equal("30111222", detalle.CodigoAlumno);
        Assert.Equal(500, detalle.CursadaIndice);
        Assert.Equal(7m, detalle.Evaluacion1);
        Assert.Equal(7, detalle.CodigoUsuarioModificacion);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_IgnoraAlumnosQueNoEstanEnLaComision()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Comision());
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaComisionDocente?)null);
        CargaComisionDocente? capturada = null;
        _cargas.When(c => c.Agregar(Arg.Any<CargaComisionDocente>())).Do(ci => capturada = ci.Arg<CargaComisionDocente>());

        await Guardar().HandleAsync(Comando(Titular,
            new FilaCargaComisionInput { CodigoAlumno = "99999999", Evaluacion1 = 10m }), CancellationToken.None);

        Assert.Empty(capturada!.Detalles);
    }

    [Fact]
    public async Task Guardar_CargaExistente_ActualizaDetalleAunqueQuedeVacio()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Comision());
        var carga = CargaExistente();
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Guardar().HandleAsync(Comando(Titular,
            new FilaCargaComisionInput { CodigoAlumno = "30111222" }), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        var detalle = Assert.Single(carga.Detalles);
        Assert.Null(detalle.Evaluacion1);
        Assert.Equal(7, detalle.CodigoUsuarioModificacion);
        Assert.Equal(7, carga.CodigoUsuarioModificacion);
        _cargas.DidNotReceive().Agregar(Arg.Any<CargaComisionDocente>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_DocenteNoTitular_DevuelveErrorSinCommit()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Comision());
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaComisionDocente?)null);

        var resultado = await Guardar().HandleAsync(Comando(OtroDocente), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("titular", resultado.Message, StringComparison.Ordinal);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(EstadoCargaDocente.Finalizada)]
    [InlineData(EstadoCargaDocente.Efectivizada)]
    public async Task Guardar_TitularConCargaCerrada_DevuelveErrorSinCommit(EstadoCargaDocente estado)
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Comision());
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(CargaExistente(estado));

        var resultado = await Guardar().HandleAsync(Comando(Titular), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_SecretariaConCargaFinalizada_PuedeModificar()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Comision());
        var carga = CargaExistente(EstadoCargaDocente.Finalizada);
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Guardar().HandleAsync(Comando(Secretaria,
            new FilaCargaComisionInput { CodigoAlumno = "30111222", Evaluacion1 = 8m }), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(8m, carga.Detalles.Single().Evaluacion1);
        Assert.Equal(1, carga.Detalles.Single().CodigoUsuarioModificacion);
        Assert.Equal(EstadoCargaDocente.Finalizada, carga.Estado);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_ComisionInexistente_DevuelveError()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaComisionDocenteDto?)null);

        var resultado = await Guardar().HandleAsync(Comando(Titular), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_ComisionSinTitularYSinCarga_DevuelveError()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Comision(titular: null));
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaComisionDocente?)null);

        var resultado = await Guardar().HandleAsync(Comando(Secretaria), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("titular", resultado.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Guardar_ComandoInvalido_NoConsultaNada()
    {
        var resultado = await Guardar().HandleAsync(Comando(Titular,
            new FilaCargaComisionInput { CodigoAlumno = "30111222", Evaluacion1 = 11m }), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _consulta.DidNotReceiveWithAnyArgs().ObtenerAsync(default!, default);
    }

    [Fact]
    public async Task Finalizar_TitularEnBorrador_PasaAFinalizadaYCommitea()
    {
        var carga = CargaExistente();
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Finalizar().HandleAsync(
            new CambiarEstadoCargaComisionCommand { Comision = Clave, Actor = Titular }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Finalizada, carga.Estado);
        Assert.Equal(7, carga.CodigoUsuarioFinalizacion);
        Assert.NotNull(carga.FechaFinalizacion);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Finalizar_SinCarga_DevuelveError()
    {
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaComisionDocente?)null);

        var resultado = await Finalizar().HandleAsync(
            new CambiarEstadoCargaComisionCommand { Comision = Clave, Actor = Titular }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Finalizar_OtroDocente_DevuelveErrorSinCommit()
    {
        var carga = CargaExistente();
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Finalizar().HandleAsync(
            new CambiarEstadoCargaComisionCommand { Comision = Clave, Actor = OtroDocente }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Borrador, carga.Estado);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Finalizar_YaFinalizada_DevuelveError()
    {
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(CargaExistente(EstadoCargaDocente.Finalizada));

        var resultado = await Finalizar().HandleAsync(
            new CambiarEstadoCargaComisionCommand { Comision = Clave, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reabrir_Secretaria_VuelveABorradorYCommitea()
    {
        var carga = CargaExistente(EstadoCargaDocente.Finalizada);
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Reabrir().HandleAsync(
            new CambiarEstadoCargaComisionCommand { Comision = Clave, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Borrador, carga.Estado);
        Assert.Equal(1, carga.CodigoUsuarioReapertura);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reabrir_Efectivizada_VuelveABorradorConAviso()
    {
        var carga = CargaExistente(EstadoCargaDocente.Efectivizada);
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Reabrir().HandleAsync(
            new CambiarEstadoCargaComisionCommand { Comision = Clave, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Borrador, carga.Estado);
    }

    [Fact]
    public async Task Reabrir_DocenteTitular_DevuelveErrorSinCommit()
    {
        var carga = CargaExistente(EstadoCargaDocente.Finalizada);
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Reabrir().HandleAsync(
            new CambiarEstadoCargaComisionCommand { Comision = Clave, Actor = Titular }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Finalizada, carga.Estado);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reabrir_YaEnBorrador_DevuelveError()
    {
        _cargas.ObtenerPorComisionAsync(Clave, Arg.Any<CancellationToken>()).Returns(CargaExistente());

        var resultado = await Reabrir().HandleAsync(
            new CambiarEstadoCargaComisionCommand { Comision = Clave, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }
}
