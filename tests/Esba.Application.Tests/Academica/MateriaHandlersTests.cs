using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class MateriaHandlersTests
{
    private readonly IMateriaRepository _materias = Substitute.For<IMateriaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CrearMateriaHandler CrearHandler() =>
        new(_materias, new CrearMateriaValidator(), _unitOfWork);

    private ActualizarMateriaHandler ActualizarHandler() =>
        new(_materias, new ActualizarMateriaValidator(), _unitOfWork);

    private static CrearMateriaCommand ComandoAlta() => new()
    {
        CodigoCarrera = "ADM",
        Codigo = "01",
        Nombre = "Matemática I",
        Sigla = "MAT1",
        Cuatrimestre = 1,
        Orden = 1,
    };

    [Fact]
    public async Task Crear_MateriaNueva_AgregaYCommiteaUnaVez()
    {
        _materias.ObtenerAsync("01", "ADM", Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var resultado = await CrearHandler().HandleAsync(ComandoAlta(), "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        _materias.Received(1).Agregar(Arg.Any<Materia>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_CodigoDeUnDigito_LoNormalizaADosDigitos()
    {
        Materia? capturada = null;
        _materias.ObtenerAsync(Arg.Any<string>(), "ADM", Arg.Any<CancellationToken>()).Returns((Materia?)null);
        _materias.When(m => m.Agregar(Arg.Any<Materia>())).Do(ci => capturada = ci.Arg<Materia>());

        var resultado = await CrearHandler().HandleAsync(ComandoAlta() with { Codigo = "1" }, "tester", CancellationToken.None);

        Assert.Equal("01", resultado.Value);
        Assert.Equal("01", capturada!.Codigo);
    }

    [Fact]
    public async Task Crear_MapeaFlagsYCorrelativasAlFormatoLegacy()
    {
        Materia? capturada = null;
        _materias.ObtenerAsync(Arg.Any<string>(), "ADM", Arg.Any<CancellationToken>()).Returns((Materia?)null);
        _materias.When(m => m.Agregar(Arg.Any<Materia>())).Do(ci => capturada = ci.Arg<Materia>());

        await CrearHandler().HandleAsync(
            ComandoAlta() with
            {
                EsAnual = true,
                AdmitePromocion = true,
                ApruebaSinFinal = false,
                DadaDeBaja = true,
                CorrelativasCursada = ["02", "03"],
                CorrelativasFinal = ["04"],
            }, "tester", CancellationToken.None);

        Assert.True(capturada!.EsAnual);
        Assert.Equal("N", capturada.ApruebaSinFinal);
        Assert.Equal("B", capturada.Estado);
        Assert.Equal("02-03", capturada.CorrelativasCursada);
        Assert.Equal("04", capturada.CorrelativasFinal);
        Assert.Equal("tester", capturada.Usuario);
    }

    [Fact]
    public async Task Crear_MateriaDuplicada_DevuelveErrorSinCommit()
    {
        _materias.ObtenerAsync("01", "ADM", Arg.Any<CancellationToken>())
            .Returns(new Materia { Codigo = "01", CodigoCarrera = "ADM" });

        var resultado = await CrearHandler().HandleAsync(ComandoAlta(), "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _materias.DidNotReceive().Agregar(Arg.Any<Materia>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_ComandoInvalido_DevuelveErrorSinTocarRepositorio()
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoAlta() with { Nombre = "" }, "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _materias.DidNotReceive().ObtenerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_MateriaInexistente_DevuelveErrorSinCommit()
    {
        _materias.ObtenerAsync("01", "ADM", Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var resultado = await ActualizarHandler().HandleAsync(new ActualizarMateriaCommand
        {
            CodigoCarrera = "ADM",
            Codigo = "01",
            Nombre = "Matemática I",
            Sigla = "MAT1",
            Cuatrimestre = 1,
            Orden = 1,
        }, "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Actualizar_MateriaExistente_AplicaCambiosYCommitea()
    {
        var existente = new Materia { Codigo = "01", CodigoCarrera = "ADM", Nombre = "Viejo" };
        _materias.ObtenerAsync("01", "ADM", Arg.Any<CancellationToken>()).Returns(existente);

        var resultado = await ActualizarHandler().HandleAsync(new ActualizarMateriaCommand
        {
            CodigoCarrera = "ADM",
            Codigo = "01",
            Nombre = "Matemática I",
            Sigla = "MAT1",
            Cuatrimestre = 2,
            Orden = 5,
        }, "tester", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.Equal("Matemática I", existente.Nombre);
        Assert.Equal((short)2, existente.Cuatrimestre);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
