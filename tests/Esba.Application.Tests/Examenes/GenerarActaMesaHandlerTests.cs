using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Examenes;
using Esba.Application.Validators;
using NSubstitute;

namespace Esba.Application.Tests.Examenes;

public class GenerarActaMesaHandlerTests
{
    private readonly IActasQuery _actas = Substitute.For<IActasQuery>();
    private readonly IConstanciasQuery _constancias = Substitute.For<IConstanciasQuery>();
    private readonly IActaReportService _reporte = Substitute.For<IActaReportService>();
    private readonly IActaExcelService _excel = Substitute.For<IActaExcelService>();

    private GenerarActaMesaHandler CrearHandler() =>
        new(new GenerarActaMesaValidator(), _actas, _constancias, _reporte, _excel);

    private static GenerarActaMesaCommand Comando(int mesa = 10, string tipo = "FINAL") =>
        new() { CodigoCarrera = "TEC", Mesa = mesa, TipoExamen = tipo };

    [Fact]
    public async Task GenerarPdf_MesaCero_DevuelveError_YNoConsulta()
    {
        var resultado = await CrearHandler().GenerarPdfAsync(Comando(mesa: 0), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _actas.DidNotReceiveWithAnyArgs().ObtenerCabeceraMesaAsync(default, default!, default);
    }

    [Fact]
    public async Task GenerarPdf_MesaInexistente_DevuelveError()
    {
        _actas.ObtenerCabeceraMesaAsync(10, "TEC", Arg.Any<CancellationToken>())
            .Returns((ActaMesaCabeceraDto?)null);

        var resultado = await CrearHandler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No hay datos para mostrar.", resultado.Message);
        _reporte.DidNotReceiveWithAnyArgs().GenerarActaMesa(default!);
    }

    [Fact]
    public async Task GenerarPdf_MesaConDatos_ArmaModeloConTituloYAlumnos()
    {
        _actas.ObtenerCabeceraMesaAsync(10, "TEC", Arg.Any<CancellationToken>())
            .Returns(new ActaMesaCabeceraDto { Docente = "Tribunal", DescripcionMateria = "Mat 1", Cutuco = 111, Dia = 5, Mes = 7, Anio = 2026 });
        _constancias.ObtenerDatosCarreraAsync("TEC", Arg.Any<CancellationToken>())
            .Returns(new CarreraConstanciaDto { Nombre = "Tecnicatura" });
        _actas.ObtenerAlumnosMesaAsync(10, "TEC", "FINAL", Arg.Any<CancellationToken>())
            .Returns([new ActaAlumnoDto { CodigoAlumno = "A1", Apellido = "Pérez", Nombre = "Ana", PermisoExamen = 7 }]);

        ActaMesaModel? capturado = null;
        _reporte.GenerarActaMesa(Arg.Do<ActaMesaModel>(m => capturado = m)).Returns([1]);

        var resultado = await CrearHandler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.NotNull(capturado);
        Assert.Equal("ACTA DE EXAMEN FINAL", capturado!.Titulo);
        Assert.Equal("Tecnicatura", capturado.CarreraLarga);
        Assert.Single(capturado.Alumnos);
        Assert.Equal(7, capturado.Alumnos[0].PermisoExamen);
    }
}
