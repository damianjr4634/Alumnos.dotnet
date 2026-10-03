using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Application.Features.AreaDocente;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using FluentValidation.TestHelper;
using NSubstitute;

namespace Esba.Application.Tests.AreaDocente;

/// <summary>
/// Precarga por mesa (hito 19): mismas reglas que la comisión (titular en borrador o
/// secretaría; finalizar; reabrir solo secretaría) más la semántica nota/ausente.
/// </summary>
public class CargaMesaHandlersTests
{
    private readonly ICargaMesaDocenteRepository _cargas = Substitute.For<ICargaMesaDocenteRepository>();
    private readonly ICargaMesaDocenteQuery _consulta = Substitute.For<ICargaMesaDocenteQuery>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly GuardarCargaMesaValidator _validator = new();

    private static readonly ClaveMesa Clave = new() { CodigoCarrera = "TER", NumeroMesa = 5001 };
    private static readonly ActorCargaDocente Titular = new() { CodigoUsuario = 7, CodigoDocente = "017" };
    private static readonly ActorCargaDocente OtroDocente = new() { CodigoUsuario = 8, CodigoDocente = "018" };
    private static readonly ActorCargaDocente Secretaria = new() { CodigoUsuario = 1, EsSecretaria = true };

    private GuardarCargaMesaHandler Guardar() => new(_cargas, _consulta, _validator, _unitOfWork, TimeProvider.System);

    private FinalizarCargaMesaHandler Finalizar() => new(_cargas, _unitOfWork, TimeProvider.System);

    private ReabrirCargaMesaHandler Reabrir() => new(_cargas, _unitOfWork, TimeProvider.System);

    private static CargaMesaDocenteDto Mesa(string? titular = "017") => new()
    {
        CodigoCarrera = "TER", NumeroMesa = 5001, CodigoMateria = "01", CodigoDocenteTitular = titular,
        Alumnos =
        [
            new AlumnoCargaMesaDto { CodigoAlumno = "30111222", CodigoMateria = "01", PermisoIndice = 900 },
            new AlumnoCargaMesaDto { CodigoAlumno = "30333444", CodigoMateria = "01", PermisoIndice = 901 },
        ],
    };

    private static CargaMesaDocente CargaExistente(EstadoCargaDocente estado = EstadoCargaDocente.Borrador) => new()
    {
        Id = 20, CodigoCarrera = "TER", NumeroMesa = 5001, CodigoDocente = "017", Estado = estado,
        Detalles = [new CargaMesaDocenteDetalle { Id = 1, CargaId = 20, CodigoAlumno = "30111222   ", CodigoMateria = "01", Nota = 4m }],
    };

    private static GuardarCargaMesaCommand Comando(ActorCargaDocente actor, params FilaCargaMesaInput[] filas) => new()
    {
        Mesa = Clave, Actor = actor, Filas = filas,
    };

    private static FilaCargaMesaInput Fila(string alumno = "30111222", decimal? nota = 8m, bool ausente = false) => new()
    {
        CodigoAlumno = alumno, CodigoMateria = "01", Nota = nota, Ausente = ausente,
    };

    [Fact]
    public void Validador_NotaFueraDeRango_Falla() =>
        _validator.TestValidate(Comando(Titular, Fila(nota: 11m))).ShouldHaveValidationErrorFor("Filas[0].Nota");

    [Fact]
    public void Validador_AusenteConNota_Falla() =>
        _validator.TestValidate(Comando(Titular, Fila(nota: 7m, ausente: true))).ShouldHaveValidationErrorFor("Filas[0].Nota");

    [Fact]
    public void Validador_AusenteSinNota_Pasa() =>
        _validator.TestValidate(Comando(Titular, Fila(nota: null, ausente: true))).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validador_NotaDecimalEnRango_Pasa() =>
        _validator.TestValidate(Comando(Titular, Fila(nota: 7.5m))).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public async Task Guardar_PrimeraVezPorElTitular_CreaCabeceraYDetallesConPermisoYCommitea()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Mesa());
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaMesaDocente?)null);
        CargaMesaDocente? capturada = null;
        _cargas.When(c => c.Agregar(Arg.Any<CargaMesaDocente>())).Do(ci => capturada = ci.Arg<CargaMesaDocente>());

        var resultado = await Guardar().HandleAsync(Comando(Titular,
            Fila("30111222", 8m),
            Fila("30333444", null, ausente: true),
            new FilaCargaMesaInput { CodigoAlumno = "99999999", CodigoMateria = "01", Nota = 10m }), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("017", capturada!.CodigoDocente);
        Assert.Equal(5001, capturada.NumeroMesa);
        Assert.Equal(2, capturada.Detalles.Count); // el alumno sin permiso se ignora
        var aprobado = capturada.Detalles.Single(d => d.CodigoAlumno == "30111222");
        Assert.Equal(8m, aprobado.Nota);
        Assert.False(aprobado.Ausente);
        Assert.Equal(900, aprobado.PermisoIndice);
        var ausente = capturada.Detalles.Single(d => d.CodigoAlumno == "30333444");
        Assert.True(ausente.Ausente);
        Assert.Null(ausente.Nota);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_FilaVaciaNueva_NoSeCrea()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Mesa());
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaMesaDocente?)null);
        CargaMesaDocente? capturada = null;
        _cargas.When(c => c.Agregar(Arg.Any<CargaMesaDocente>())).Do(ci => capturada = ci.Arg<CargaMesaDocente>());

        await Guardar().HandleAsync(Comando(Titular, Fila(nota: null)), CancellationToken.None);

        Assert.Empty(capturada!.Detalles);
    }

    [Fact]
    public async Task Guardar_DocenteNoTitular_DevuelveErrorSinCommit()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Mesa());
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaMesaDocente?)null);

        var resultado = await Guardar().HandleAsync(Comando(OtroDocente, Fila()), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("esta mesa", resultado.Message, StringComparison.Ordinal);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_TitularConCargaFinalizada_DevuelveErrorSinCommit()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Mesa());
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns(CargaExistente(EstadoCargaDocente.Finalizada));

        var resultado = await Guardar().HandleAsync(Comando(Titular, Fila()), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_SecretariaSobreCargaFinalizada_ActualizaDetalle()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Mesa());
        var carga = CargaExistente(EstadoCargaDocente.Finalizada);
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Guardar().HandleAsync(Comando(Secretaria, Fila(nota: 9m)), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(9m, carga.Detalles.Single().Nota);
        Assert.Equal(1, carga.Detalles.Single().CodigoUsuarioModificacion);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guardar_MesaInexistente_DevuelveError()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaMesaDocenteDto?)null);

        var resultado = await Guardar().HandleAsync(Comando(Titular, Fila()), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }

    [Fact]
    public async Task Guardar_MesaSinTitular_DevuelveError()
    {
        _consulta.ObtenerAsync(Clave, Arg.Any<CancellationToken>()).Returns(Mesa(titular: null));
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaMesaDocente?)null);

        var resultado = await Guardar().HandleAsync(Comando(Secretaria, Fila()), CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("titular", resultado.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Finalizar_Titular_PasaAFinalizada()
    {
        var carga = CargaExistente();
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Finalizar().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Clave, Actor = Titular }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Finalizada, carga.Estado);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Finalizar_OtroDocente_DevuelveErrorSinCommit()
    {
        var carga = CargaExistente();
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Finalizar().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Clave, Actor = OtroDocente }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Borrador, carga.Estado);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Finalizar_SinCarga_DevuelveError()
    {
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns((CargaMesaDocente?)null);

        var resultado = await Finalizar().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Clave, Actor = Titular }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }

    [Fact]
    public async Task Reabrir_Secretaria_VuelveABorrador()
    {
        var carga = CargaExistente(EstadoCargaDocente.Finalizada);
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Reabrir().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Clave, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Borrador, carga.Estado);
    }

    [Fact]
    public async Task Reabrir_Efectivizada_VuelveABorradorConAviso()
    {
        var carga = CargaExistente(EstadoCargaDocente.Efectivizada);
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Reabrir().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Clave, Actor = Secretaria }, CancellationToken.None);

        Assert.Equal(OperationStatus.Warning, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Borrador, carga.Estado);
    }

    [Fact]
    public async Task Reabrir_Titular_DevuelveErrorSinCommit()
    {
        var carga = CargaExistente(EstadoCargaDocente.Finalizada);
        _cargas.ObtenerPorMesaAsync(Clave, Arg.Any<CancellationToken>()).Returns(carga);

        var resultado = await Reabrir().HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = Clave, Actor = Titular }, CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(EstadoCargaDocente.Finalizada, carga.Estado);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
