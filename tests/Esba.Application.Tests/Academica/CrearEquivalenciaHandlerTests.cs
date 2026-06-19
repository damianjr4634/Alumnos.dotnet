using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class CrearEquivalenciaHandlerTests
{
    private readonly IValidacionMateriaProcedure _validacion = Substitute.For<IValidacionMateriaProcedure>();
    private readonly IEquivalenciaNumeracionProcedure _numeracion = Substitute.For<IEquivalenciaNumeracionProcedure>();
    private readonly IAnaliticoRepository _analiticos = Substitute.For<IAnaliticoRepository>();
    private readonly IAlumnoRepository _alumnos = Substitute.For<IAlumnoRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CrearEquivalenciaHandler CrearHandler() =>
        new(new CrearEquivalenciaValidator(), _validacion, _numeracion, _analiticos, _alumnos, _unitOfWork);

    private static CrearEquivalenciaCommand Comando(TipoActuacionEquivalencia tipo = TipoActuacionEquivalencia.Interna) => new()
    {
        CodigoCarrera = "TER",
        CodigoAlumno = "DNI30123456",
        CodigoMateria = "07",
        TipoActuacion = tipo,
        NumeroDgegp = tipo == TipoActuacionEquivalencia.Dgegp ? "555/24" : null,
        InstitutoOrigen = "Instituto Origen",
        Documento = DocumentoEquivalencia.Analitico,
    };

    private void ConAlumno()
    {
        _alumnos.ObtenerAsync("TER", "DNI30123456", Arg.Any<CancellationToken>())
            .Returns(new Alumno { Codigo = "DNI30123456", CodigoCarrera = "TER", Apellido = "García", Matriz = "01234" });
    }

    private void MateriaValida() =>
        _validacion.ValidarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<char>(), Arg.Any<CancellationToken>())
            .Returns(Result.Ok(true));

    private void NumeroInterno(string numeroEntero, bool esNuevo) =>
        _numeracion.ObtenerProximoNumeroAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new NumeroEquivalenciaDto { NumeroFormateado = "0000000000123/24", NumeroEntero = numeroEntero, EsNuevo = esNuevo });

    [Fact]
    public async Task Crear_Interna_NumeroNuevo_InsertaEquivalenciaYConfirmaNumeracion()
    {
        ConAlumno();
        MateriaValida();
        NumeroInterno("000000000012324", esNuevo: true);
        Analitico? agregado = null;
        _analiticos.When(a => a.Agregar(Arg.Any<Analitico>())).Do(ci => agregado = ci.Arg<Analitico>());

        var resultado = await CrearHandler().HandleAsync(Comando(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(agregado);
        Assert.Equal("EQUIVALENCIA", agregado.Condicion);
        Assert.Equal("000000000012324", agregado.ActaInterna);
        Assert.Null(agregado.ActaDge);
        Assert.Equal("García", agregado.Apellido);
        Assert.Equal("A", agregado.Ac);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        // Número nuevo interno: se confirma el consumo con el entero (sin el sufijo de año).
        await _numeracion.Received(1).GrabarNumeroAsync(123, "TER", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_Interna_NumeroExistente_NoConfirmaNumeracion()
    {
        ConAlumno();
        MateriaValida();
        NumeroInterno("000000000012324", esNuevo: false);

        var resultado = await CrearHandler().HandleAsync(Comando(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        await _numeracion.DidNotReceive().GrabarNumeroAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_Dgegp_UsaActaDgeYNoTocaNumeracionInterna()
    {
        ConAlumno();
        MateriaValida();
        Analitico? agregado = null;
        _analiticos.When(a => a.Agregar(Arg.Any<Analitico>())).Do(ci => agregado = ci.Arg<Analitico>());

        var resultado = await CrearHandler().HandleAsync(Comando(TipoActuacionEquivalencia.Dgegp), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(agregado);
        Assert.Equal("55524", agregado.ActaDge);   // "555/24" sin separador; el trigger LPADea
        Assert.Null(agregado.ActaInterna);
        await _numeracion.DidNotReceive().ObtenerProximoNumeroAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _numeracion.DidNotReceive().GrabarNumeroAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_MateriaInvalida_DevuelveErrorYNoInserta()
    {
        ConAlumno();
        _validacion.ValidarAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<char>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error<bool>("La materia ya está aprobada por final/Equivalencia"));

        var resultado = await CrearHandler().HandleAsync(Comando(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _analiticos.DidNotReceive().Agregar(Arg.Any<Analitico>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Crear_SinInstitutoOrigen_DevuelveErrorDeValidacion()
    {
        ConAlumno();
        MateriaValida();

        var resultado = await CrearHandler().HandleAsync(Comando() with { InstitutoOrigen = "  " }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _analiticos.DidNotReceive().Agregar(Arg.Any<Analitico>());
    }

    [Fact]
    public async Task Crear_AlumnoInexistente_DevuelveError()
    {
        MateriaValida();
        _alumnos.ObtenerAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Alumno?)null);

        var resultado = await CrearHandler().HandleAsync(Comando(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        _analiticos.DidNotReceive().Agregar(Arg.Any<Analitico>());
    }
}
