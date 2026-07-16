using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Application.DTOs.Certificados;
using Esba.Application.Features.Asistencias;
using Esba.Application.Validators;
using Esba.Domain.Asistencias;
using NSubstitute;

namespace Esba.Application.Tests.Asistencias;

public class GenerarCarpetaComisionHandlerTests
{
    private readonly ICarpetaComisionQuery _carpeta = Substitute.For<ICarpetaComisionQuery>();
    private readonly IConstanciasQuery _constancias = Substitute.For<IConstanciasQuery>();
    private readonly ICarpetaComisionReportService _reporte = Substitute.For<ICarpetaComisionReportService>();
    private readonly ICarpetaComisionExcelService _excel = Substitute.For<ICarpetaComisionExcelService>();

    private GenerarCarpetaComisionHandler CrearHandler() =>
        new(new GenerarCarpetaComisionValidator(), _carpeta, _constancias, _reporte, _excel, TimeProvider.System);

    private static GenerarCarpetaComisionCommand Comando(
        TipoCarpetaComision tipo = TipoCarpetaComision.Asistencia,
        string? cuatrimestre = "1/24",
        short? cutuco = null) => new()
    {
        Tipo = tipo,
        CodigoCarrera = "TEC",
        CuatrimestreAnio = cuatrimestre!,
        Cutuco = cutuco,
        CodigoMateria = null,
    };

    [Fact]
    public async Task GenerarPdf_SinCuatrimestre_DevuelveError_YNoConsultaComisiones()
    {
        var resultado = await CrearHandler().GenerarPdfAsync(Comando(cuatrimestre: ""), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        await _carpeta.DidNotReceiveWithAnyArgs().ObtenerComisionesAsync(
            default!, default!, default, default, default);
    }

    [Fact]
    public async Task GenerarPdf_ComisionNegativa_DevuelveError()
    {
        var resultado = await CrearHandler().GenerarPdfAsync(Comando(cutuco: -1), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La comisión debe ser un número positivo.", resultado.Message);
    }

    [Fact]
    public async Task GenerarPdf_SinComisiones_DevuelveError_YNoGeneraReporte()
    {
        _carpeta.ObtenerComisionesAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns([]);

        var resultado = await CrearHandler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("No hay datos para mostrar.", resultado.Message);
        _reporte.DidNotReceiveWithAnyArgs().GenerarCarpeta(default!);
    }

    [Fact]
    public async Task GenerarPdf_AgrupaPorComision_YSeparaRecursantes()
    {
        var cabeceras = new[]
        {
            new CarpetaComisionCabeceraDto { Cutuco = 111, CodigoMateria = "01", DescripcionMateria = "Mat 1", Docente = "Prof A", TitularSuplente = "T" },
            new CarpetaComisionCabeceraDto { Cutuco = 112, CodigoMateria = "02", DescripcionMateria = "Mat 2", Docente = "Prof B", TitularSuplente = "S" },
        };
        var alumnos = new[]
        {
            new CarpetaComisionAlumnoDto { CodigoAlumno = "A1", Apellido = "Pérez", Nombre = "Ana", Condicion = "CURSANDO", Cutuco = 111, CodigoMateria = "01" },
            new CarpetaComisionAlumnoDto { CodigoAlumno = "A2", Apellido = "Gómez", Nombre = "Luis", Condicion = "RECURSANDO", Cutuco = 111, CodigoMateria = "01" },
            new CarpetaComisionAlumnoDto { CodigoAlumno = "A3", Apellido = "Ruiz", Nombre = "Eva", Condicion = "CURSANDO", Cutuco = 112, CodigoMateria = "02" },
        };

        _constancias.ObtenerDatosCarreraAsync("TEC", Arg.Any<CancellationToken>())
            .Returns(new CarreraConstanciaDto { Nombre = "Tecnicatura" });
        _carpeta.ObtenerComisionesAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns(cabeceras);
        _carpeta.ObtenerAlumnosAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns(alumnos);

        CarpetaComisionModel? capturado = null;
        _reporte.GenerarCarpeta(Arg.Do<CarpetaComisionModel>(m => capturado = m)).Returns([1, 2, 3]);

        var resultado = await CrearHandler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal([1, 2, 3], resultado.Value);
        Assert.NotNull(capturado);
        Assert.Equal(TipoCarpetaComision.Asistencia, capturado!.Tipo);
        Assert.Equal("Tecnicatura", capturado.CarreraLarga);
        Assert.Equal(2, capturado.Secciones.Count);
        Assert.Single(capturado.Secciones[0].Cursando);          // comisión 111/01: A1
        Assert.Single(capturado.Secciones[0].Recursantes);       // comisión 111/01: A2
        Assert.Single(capturado.Secciones[1].Cursando);          // comisión 112/02: A3
        Assert.Empty(capturado.Secciones[1].Recursantes);
    }

    [Fact]
    public async Task GenerarPdf_TrabajosPracticos_PropagaElTipoAlModelo()
    {
        _carpeta.ObtenerComisionesAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns([new CarpetaComisionCabeceraDto { Cutuco = 111, CodigoMateria = "01" }]);
        _carpeta.ObtenerAlumnosAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns([]);

        CarpetaComisionModel? capturado = null;
        _reporte.GenerarCarpeta(Arg.Do<CarpetaComisionModel>(m => capturado = m)).Returns([9]);

        var resultado = await CrearHandler().GenerarPdfAsync(
            Comando(TipoCarpetaComision.TrabajosPracticos), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(TipoCarpetaComision.TrabajosPracticos, capturado!.Tipo);
    }

    [Fact]
    public async Task GenerarExcel_Asistencia_DevuelveError_YNoConsultaNada()
    {
        var resultado = await CrearHandler().GenerarExcelAsync(
            Comando(TipoCarpetaComision.Asistencia), CancellationToken.None);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La carpeta de asistencia no tiene exportación a Excel.", resultado.Message);
        await _carpeta.DidNotReceiveWithAnyArgs().ObtenerComisionesAsync(
            default!, default!, default, default, default);
        _excel.DidNotReceiveWithAnyArgs().GenerarCarpeta(default!);
    }

    [Fact]
    public async Task GenerarExcel_PlanillaProfesores_GeneraConElServicioExcel()
    {
        _carpeta.ObtenerComisionesAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns([new CarpetaComisionCabeceraDto { Cutuco = 111, CodigoMateria = "01" }]);
        _carpeta.ObtenerAlumnosAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns([]);

        CarpetaComisionModel? capturado = null;
        var esperado = new CarpetaComisionExcelResultado
        {
            Contenido = [7],
            NombreArchivo = "Notas_111_Mat.xlsx",
            EsZip = false,
        };
        _excel.GenerarCarpeta(Arg.Do<CarpetaComisionModel>(m => capturado = m)).Returns(esperado);

        var resultado = await CrearHandler().GenerarExcelAsync(
            Comando(TipoCarpetaComision.PlanillaProfesores), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Same(esperado, resultado.Value);
        Assert.Equal(TipoCarpetaComision.PlanillaProfesores, capturado!.Tipo);
        _reporte.DidNotReceiveWithAnyArgs().GenerarCarpeta(default!);
    }

    [Fact]
    public async Task GenerarPdf_SinNombreDeCarrera_UsaElCodigoComoTitulo()
    {
        _constancias.ObtenerDatosCarreraAsync("TEC", Arg.Any<CancellationToken>())
            .Returns((CarreraConstanciaDto?)null);
        _carpeta.ObtenerComisionesAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns([new CarpetaComisionCabeceraDto { Cutuco = 111, CodigoMateria = "01" }]);
        _carpeta.ObtenerAlumnosAsync("TEC", "1/24", null, null, Arg.Any<CancellationToken>())
            .Returns([]);

        CarpetaComisionModel? capturado = null;
        _reporte.GenerarCarpeta(Arg.Do<CarpetaComisionModel>(m => capturado = m)).Returns([9]);

        var resultado = await CrearHandler().GenerarPdfAsync(Comando(), CancellationToken.None);

        Assert.True(resultado.IsSuccess);
        Assert.Equal("TEC", capturado!.CarreraLarga);
    }
}
