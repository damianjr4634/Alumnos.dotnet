using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using NSubstitute;

namespace Esba.Application.Tests.Academica;

public class InscribirEnMateriaHandlerTests
{
    private readonly ICursadaRepository _cursadas = Substitute.For<ICursadaRepository>();
    private readonly IAlumnoRepository _alumnos = Substitute.For<IAlumnoRepository>();
    private readonly IMateriaRepository _materias = Substitute.For<IMateriaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private InscribirEnMateriaHandler CrearHandler() =>
        new(_cursadas, _alumnos, _materias, new InscribirEnMateriaValidator(), _unitOfWork);

    private static InscribirEnMateriaCommand ComandoValido() => new()
    {
        CodigoCarrera = "ADM",
        CodigoAlumno = "DNI30123456",
        CodigoMateria = "01",
        Turno = 1,
        NumeroComision = 2,
        CuatrimestreAnio = "126",
        Condicion = "CURSANDO",
    };

    private void ConAlumnoYMateria()
    {
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>())
            .Returns(new Alumno
            {
                Codigo = "DNI30123456",
                CodigoCarrera = "ADM",
                Apellido = "García",
                Matriz = "01234",
                Carrera = new Carrera
                {
                    Codigo = "ADM",
                    DirectorEstudios = "X",
                    Grupo = "G1",
                    Idioma = "Castellano",
                    Instituto = "ESBA",
                    Caracteristica = "A-781",
                },
            });
        _materias.ObtenerAsync("01", "ADM", Arg.Any<CancellationToken>())
            .Returns(new Materia { Codigo = "01", CodigoCarrera = "ADM", Nombre = "Matemática" });
    }

    [Fact]
    public async Task Inscribir_AlumnoYMateriaValidos_CreaCursadaFielAlLegacyYCommitea()
    {
        ConAlumnoYMateria();
        Cursada? agregada = null;
        _cursadas.When(c => c.Agregar(Arg.Any<Cursada>())).Do(ci => agregada = ci.Arg<Cursada>());

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, resultado.Status);
        Assert.NotNull(agregada);
        Assert.Equal((short?)912, agregada.Cutuco);             // 900 + 1×10 + 2
        Assert.Equal("126", agregada.CuatrimestreAnio);
        Assert.Equal("CURSANDO", agregada.Condicion);
        Assert.Equal("García", agregada.Apellido);      // denormalizado del alumno
        Assert.Equal("01234", agregada.Matriz);
        Assert.Equal("ESBA", agregada.Instituto);       // de la carrera
        Assert.Equal(0, agregada.NumeroRegistro);       // NREG=0 como el legacy
        Assert.Equal("damian", agregada.Usuario);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Inscribir_AlumnoYaInscripto_DevuelveError()
    {
        ConAlumnoYMateria();
        _cursadas.ObtenerAsync("ADM", "DNI30123456", "01", Arg.Any<CancellationToken>())
            .Returns(new Cursada
            {
                CodigoCarrera = "ADM",
                CodigoAlumno = "DNI30123456",
                CodigoMateria = "01",
                Condicion = "REGULAR",
            });

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Contains("ya está inscripto", resultado.Message, StringComparison.OrdinalIgnoreCase);
        await _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Inscribir_AlumnoInexistente_DevuelveError()
    {
        _alumnos.ObtenerAsync("ADM", "DNI30123456", Arg.Any<CancellationToken>())
            .Returns((Alumno?)null);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("El alumno no existe.", resultado.Message);
    }

    [Fact]
    public async Task Inscribir_MateriaInexistente_DevuelveError()
    {
        ConAlumnoYMateria();
        _materias.ObtenerAsync("01", "ADM", Arg.Any<CancellationToken>())
            .Returns((Materia?)null);

        var resultado = await CrearHandler().HandleAsync(ComandoValido(), "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal("La materia no existe en la carrera.", resultado.Message);
    }

    [Fact]
    public async Task Inscribir_CondicionInvalida_DevuelveErrorSinTocarRepositorios()
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoValido() with { Condicion = "REGULAR" }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
        await _alumnos.DidNotReceiveWithAnyArgs().ObtenerAsync(default!, default!, default);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(10, 0)]
    [InlineData(0, 10)]
    public async Task Inscribir_TurnoOComisionFueraDeRango_DevuelveError(int turno, int comision)
    {
        var resultado = await CrearHandler().HandleAsync(
            ComandoValido() with { Turno = turno, NumeroComision = comision }, "damian", CancellationToken.None);

        Assert.Equal(OperationStatus.Error, resultado.Status);
    }
}
