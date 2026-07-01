using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Application.DTOs.Examenes;
using Esba.Application.Features.Examenes;
using Esba.Application.Validators;
using Esba.Domain.Examenes;
using NSubstitute;

namespace Esba.Application.Tests.Examenes;

public class GenerarActaComisionHandlerTests
{
    private readonly IActasQuery _actas = Substitute.For<IActasQuery>();
    private readonly IConstanciasQuery _constancias = Substitute.For<IConstanciasQuery>();
    private readonly IActaReportService _reporte = Substitute.For<IActaReportService>();
    private readonly IActaExcelService _excel = Substitute.For<IActaExcelService>();

    private GenerarActaComisionHandler CrearHandler() =>
        new(new GenerarActaComisionValidator(), _actas, _constancias, _reporte, _excel);

    private static GenerarActaComisionCommand Comando(
        TipoActaComision tipo = TipoActaComision.ARegular,
        string? cuatrimestre = "1/24") => new()
    {
        Tipo = tipo,
        CodigoCarrera = "TEC",
        CuatrimestreAnio = cuatrimestre!,
        Cutuco = null,
        CodigoMateria = null,
    };

    [Fact]
    public async Task GenerarPdf_SinCuatrimestre_DevuelveError_YNoConsultaActas()
    {
        var resultado = await CrearHandler().GenerarPdfAsync(Comando(cuatrimestre: ""), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _actas.DidNotReceiveWithAnyArgs().ObtenerCabecerasComisionAsync(
            default!, default!, default, default, default!, default, default);
    }

    [Fact]
    public async Task GenerarPdf_SinCabeceras_DevuelveError_YNoGeneraReporte()
    {
        _actas.ObtenerCabecerasComisionAsync(
            "TEC", "1/24", null, null, Arg.Any<IReadOnlyList<string>>(), true, Arg.Any<CancellationToken>())
            .Returns([]);

        var resultado = await CrearHandler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No hay datos para mostrar.", resultado.Message);
        _reporte.DidNotReceiveWithAnyArgs().GenerarActaComision(default!);
    }

    [Fact]
    public async Task GenerarPdf_ARegular_AgrupaAlumnosPorComision_YUsaFiltroPorCondicion()
    {
        var cabeceras = new[]
        {
            new ActaComisionCabeceraDto { Cutuco = 111, CodigoMateria = "01", DescripcionMateria = "Mat 1", Docente = "Prof A" },
            new ActaComisionCabeceraDto { Cutuco = 112, CodigoMateria = "02", DescripcionMateria = "Mat 2", Docente = "Prof B" },
        };
        var alumnos = new[]
        {
            new ActaAlumnoDto { CodigoAlumno = "A1", Apellido = "Pérez", Nombre = "Ana", Cutuco = 111, CodigoMateria = "01" },
            new ActaAlumnoDto { CodigoAlumno = "A2", Apellido = "Gómez", Nombre = "Luis", Cutuco = 111, CodigoMateria = "01" },
        };

        _constancias.ObtenerDatosCarreraAsync("TEC", Arg.Any<CancellationToken>())
            .Returns(new CarreraConstanciaDto { Nombre = "Tecnicatura" });
        _actas.ObtenerCabecerasComisionAsync(
            "TEC", "1/24", null, null, Arg.Any<IReadOnlyList<string>>(), true, Arg.Any<CancellationToken>())
            .Returns(cabeceras);
        _actas.ObtenerAlumnosComisionAsync(
            "TEC", "1/24", null, null, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns(alumnos);

        ActaComisionModel? capturado = null;
        _reporte.GenerarActaComision(Arg.Do<ActaComisionModel>(m => capturado = m)).Returns([1, 2, 3]);

        var resultado = await CrearHandler().GenerarPdfAsync(Comando(TipoActaComision.ARegular), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal([1, 2, 3], resultado.Value);
        Assert.NotNull(capturado);
        Assert.Equal("ACTA DE EXAMENES DE ALUMNOS A/REGULAR", capturado!.Titulo);
        Assert.Equal("Tecnicatura", capturado.CarreraLarga);
        Assert.True(capturado.MuestraCorrespondienteCuatrimestre);
        Assert.Equal(2, capturado.Secciones.Count);
        Assert.Equal(2, capturado.Secciones[0].Alumnos.Count);   // comisión 111/01
        Assert.Empty(capturado.Secciones[1].Alumnos);            // comisión 112/02 sin alumnos
    }

    [Fact]
    public async Task GenerarExcel_Examenes_NoFiltraCabeceraPorCondicion()
    {
        _actas.ObtenerCabecerasComisionAsync(
            "TEC", "1/24", null, null, Arg.Any<IReadOnlyList<string>>(), false, Arg.Any<CancellationToken>())
            .Returns([new ActaComisionCabeceraDto { Cutuco = 111, CodigoMateria = "01" }]);
        _actas.ObtenerAlumnosComisionAsync(
            "TEC", "1/24", null, null, Arg.Any<IReadOnlyList<string>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _excel.GenerarActaComision(Arg.Any<ActaComisionModel>()).Returns([9]);

        var resultado = await CrearHandler().GenerarExcelAsync(Comando(TipoActaComision.Examenes), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        // Verifica que se pidió la cabecera con filtrarPorCondicion=false (rama de exámenes).
        await _actas.Received(1).ObtenerCabecerasComisionAsync(
            "TEC", "1/24", null, null, Arg.Any<IReadOnlyList<string>>(), false, Arg.Any<CancellationToken>());
    }
}
