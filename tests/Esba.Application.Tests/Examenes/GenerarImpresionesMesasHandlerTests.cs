using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Examenes;
using Esba.Application.Validators;
using Esba.Domain.Examenes;
using NSubstitute;

namespace Esba.Application.Tests.Examenes;

public class GenerarImpresionesMesasHandlerTests
{
    private static readonly DateTimeOffset Hoy = new(2026, 9, 29, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Desde = new(2026, 12, 1);
    private static readonly DateOnly Hasta = new(2026, 12, 20);

    private sealed class RelojFijo(DateTimeOffset ahora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => ahora;

        public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    }

    private readonly IImpresionesMesasQuery _query = Substitute.For<IImpresionesMesasQuery>();
    private readonly ICitacionDocentesReportService _citacion = Substitute.For<ICitacionDocentesReportService>();
    private readonly IParteDiarioMesasReportService _parteDiario = Substitute.For<IParteDiarioMesasReportService>();

    private GenerarImpresionesMesasHandler CrearHandler() => new(
        new GenerarCitacionDocentesValidator(),
        new GenerarParteDiarioMesasValidator(),
        _query,
        _citacion,
        _parteDiario,
        new RelojFijo(Hoy));

    private static GenerarCitacionDocentesCommand ComandoCitacion(
        DateOnly? hasta = null,
        string? profDesde = null,
        string? profHasta = null,
        FirmanteCitacion firmante = FirmanteCitacion.Rector,
        string carreraFirma = "TEC",
        IReadOnlyList<string>? carreras = null) => new()
    {
        FechaDesde = Desde,
        FechaHasta = hasta ?? Hasta,
        CodigoProfesorDesde = profDesde,
        CodigoProfesorHasta = profHasta,
        CodigosCarrera = carreras ?? [],
        Firmante = firmante,
        CodigoCarreraFirma = carreraFirma,
    };

    private static GenerarParteDiarioMesasCommand ComandoParte(DateOnly? hasta = null, string? carrera = null) => new()
    {
        FechaDesde = Desde,
        FechaHasta = hasta ?? Hasta,
        CodigoCarrera = carrera,
    };

    private static AutoridadesCarreraDto Autoridades => new()
    {
        Rector = "RECTORA UNO", Secretaria = "SECRETARIA DOS", DirectorEstudios = "DIRECTOR TRES",
    };

    private static MesaCitacionDto Mesa(string profesor, int mesa, string carrera = "TEC") => new()
    {
        CodigoProfesor = profesor, FechaExamen = Desde, Hora = 930, Materia = "MAT", Mesa = mesa, Aula = 3, CodigoCarrera = carrera,
    };

    private static ParteDiarioMesaDto MesaParte(DateOnly fecha, string carrera, int mesa) => new()
    {
        FechaExamen = fecha, CodigoCarrera = carrera, NombreCarrera = $"Carrera {carrera}", Mesa = mesa,
        Hora = 1830, Materia = "MAT", Titular = "T", Comision1 = 111, CantidadAlumnos = 5, Aula = 2,
    };

    // ---- Citación ----

    [Fact]
    public async Task Citacion_FechaHastaAnterior_DevuelveError_YNoConsulta()
    {
        var resultado = await CrearHandler().GenerarCitacionPdfAsync(
            ComandoCitacion(hasta: Desde.AddDays(-1)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La fecha hasta no puede ser anterior a la fecha desde.", resultado.Message);
        await _query.DidNotReceiveWithAnyArgs().ObtenerAutoridadesAsync(default!, default);
    }

    [Fact]
    public async Task Citacion_SoloUnExtremoDeDocente_DevuelveError()
    {
        var resultado = await CrearHandler().GenerarCitacionPdfAsync(
            ComandoCitacion(profDesde: "001"), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Indicá docente desde y hasta, o ninguno de los dos.", resultado.Message);
    }

    [Fact]
    public async Task Citacion_DocenteHastaMenorQueDesde_DevuelveError()
    {
        var resultado = await CrearHandler().GenerarCitacionPdfAsync(
            ComandoCitacion(profDesde: "050", profHasta: "010"), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El docente hasta no puede ser anterior al docente desde.", resultado.Message);
    }

    [Fact]
    public async Task Citacion_SinCarreraDeFirma_DevuelveError()
    {
        var resultado = await CrearHandler().GenerarCitacionPdfAsync(
            ComandoCitacion(carreraFirma: ""), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Elegí la carrera cuyas autoridades firman la citación.", resultado.Message);
    }

    [Fact]
    public async Task Citacion_CarreraDeFirmaInexistente_DevuelveError()
    {
        _query.ObtenerAutoridadesAsync("TEC", Arg.Any<CancellationToken>()).Returns((AutoridadesCarreraDto?)null);

        var resultado = await CrearHandler().GenerarCitacionPdfAsync(ComandoCitacion(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La carrera de firma no existe.", resultado.Message);
    }

    [Fact]
    public async Task Citacion_SinDocentes_DevuelveError_YNoGeneraReporte()
    {
        _query.ObtenerAutoridadesAsync("TEC", Arg.Any<CancellationToken>()).Returns(Autoridades);
        _query.ObtenerDocentesCitadosAsync(Desde, Hasta, null, null, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var resultado = await CrearHandler().GenerarCitacionPdfAsync(ComandoCitacion(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No hay datos para mostrar.", resultado.Message);
        _citacion.DidNotReceiveWithAnyArgs().GenerarCitacion(default!);
    }

    [Fact]
    public async Task Citacion_AgrupaMesasPorDocente_YTomaElFirmanteDeLaCarrera()
    {
        var carreras = new[] { "TEC", "BAC" };
        _query.ObtenerAutoridadesAsync("TEC", Arg.Any<CancellationToken>()).Returns(Autoridades);
        _query.ObtenerDocentesCitadosAsync(Desde, Hasta, "001", "099", carreras, Arg.Any<CancellationToken>())
            .Returns([
                new DocenteCitadoDto { CodigoProfesor = "001", Docente = "PÉREZ, ANA" },
                new DocenteCitadoDto { CodigoProfesor = "002", Docente = "GÓMEZ, LUIS" },
            ]);
        _query.ObtenerMesasCitacionAsync(Desde, Hasta, "001", "099", carreras, Arg.Any<CancellationToken>())
            .Returns([Mesa("001", 10), Mesa("001", 11, "BAC"), Mesa("002", 10)]);

        CitacionDocentesModel? capturado = null;
        _citacion.GenerarCitacion(Arg.Do<CitacionDocentesModel>(m => capturado = m)).Returns([1, 2]);

        var resultado = await CrearHandler().GenerarCitacionPdfAsync(
            ComandoCitacion(profDesde: "001", profHasta: "099", firmante: FirmanteCitacion.Secretaria, carreras: carreras),
            CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal([1, 2], resultado.Value);
        Assert.NotNull(capturado);
        Assert.Equal(new DateOnly(2026, 9, 29), capturado!.FechaEmision);
        Assert.Equal(FirmanteCitacion.Secretaria, capturado.Firmante);
        Assert.Equal("SECRETARIA DOS", capturado.NombreFirmante);
        Assert.Equal(2, capturado.Docentes.Count);
        Assert.Equal(2, capturado.Docentes[0].Mesas.Count);      // PÉREZ: mesas 10 y 11
        Assert.Single(capturado.Docentes[1].Mesas);              // GÓMEZ: mesa 10
    }

    [Theory]
    [InlineData(FirmanteCitacion.Rector, "RECTORA UNO")]
    [InlineData(FirmanteCitacion.DirectorEstudios, "DIRECTOR TRES")]
    public async Task Citacion_NombreDelFirmanteSegunLaOpcion(FirmanteCitacion firmante, string esperado)
    {
        _query.ObtenerAutoridadesAsync("TEC", Arg.Any<CancellationToken>()).Returns(Autoridades);
        _query.ObtenerDocentesCitadosAsync(Desde, Hasta, null, null, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns([new DocenteCitadoDto { CodigoProfesor = "001", Docente = "PÉREZ, ANA" }]);
        _query.ObtenerMesasCitacionAsync(Desde, Hasta, null, null, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);

        CitacionDocentesModel? capturado = null;
        _citacion.GenerarCitacion(Arg.Do<CitacionDocentesModel>(m => capturado = m)).Returns([1]);

        var resultado = await CrearHandler().GenerarCitacionPdfAsync(ComandoCitacion(firmante: firmante), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(esperado, capturado!.NombreFirmante);
        Assert.Empty(capturado.Docentes[0].Mesas);
    }

    // ---- Parte diario ----

    [Fact]
    public async Task ParteDiario_FechaHastaAnterior_DevuelveError()
    {
        var resultado = await CrearHandler().GenerarParteDiarioPdfAsync(
            ComandoParte(hasta: Desde.AddDays(-1)), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _query.DidNotReceiveWithAnyArgs().ObtenerParteDiarioAsync(default, default, default, default);
    }

    [Fact]
    public async Task ParteDiario_SinMesas_DevuelveError_YNoGeneraReporte()
    {
        _query.ObtenerParteDiarioAsync(Desde, Hasta, null, Arg.Any<CancellationToken>()).Returns([]);

        var resultado = await CrearHandler().GenerarParteDiarioPdfAsync(ComandoParte(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No hay datos para mostrar.", resultado.Message);
        _parteDiario.DidNotReceiveWithAnyArgs().GenerarParteDiario(default!);
    }

    [Fact]
    public async Task ParteDiario_CortaPorFechaYCarrera_EnElOrdenDeLaQuery()
    {
        var dia2 = Desde.AddDays(1);
        _query.ObtenerParteDiarioAsync(Desde, Hasta, "TEC", Arg.Any<CancellationToken>()).Returns(
        [
            MesaParte(Desde, "BAC", 1),
            MesaParte(Desde, "BAC", 2),
            MesaParte(Desde, "TEC", 3),
            MesaParte(dia2, "TEC", 4),
        ]);

        ParteDiarioMesasModel? capturado = null;
        _parteDiario.GenerarParteDiario(Arg.Do<ParteDiarioMesasModel>(m => capturado = m)).Returns([9]);

        var resultado = await CrearHandler().GenerarParteDiarioPdfAsync(ComandoParte(carrera: " TEC "), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(capturado);
        Assert.Equal(2026, capturado!.CicloLectivo);
        Assert.Equal(2, capturado.Dias.Count);

        var primerDia = capturado.Dias[0];
        Assert.Equal(Desde, primerDia.Fecha);
        Assert.Equal(["BAC", "TEC"], primerDia.Carreras.Select(c => c.CodigoCarrera));
        Assert.Equal("Carrera BAC", primerDia.Carreras[0].NombreCarrera);
        Assert.Equal(2, primerDia.Carreras[0].Mesas.Count);
        Assert.Single(primerDia.Carreras[1].Mesas);

        Assert.Equal(dia2, capturado.Dias[1].Fecha);
        Assert.Single(capturado.Dias[1].Carreras);
    }
}
