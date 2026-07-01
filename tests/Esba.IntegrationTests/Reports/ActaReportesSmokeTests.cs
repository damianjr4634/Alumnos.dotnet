using Esba.Application.DTOs.Examenes;
using Esba.Domain.Examenes;
using Esba.Infrastructure.Excel;
using Esba.Infrastructure.Reports;

namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Smoke tests de los reportes de actas: QuestPDF puede arrojar en GeneratePdf ante
/// errores de maqueta y ClosedXML al guardar; verificamos que ambos producen un
/// archivo no vacío con datos realistas (incluida una sección sin alumnos, como las
/// actas de exámenes). No necesitan base de datos (sin el trait Integration).
/// </summary>
public class ActaReportesSmokeTests
{
    private static ActaComisionModel ModeloComision() => new()
    {
        Tipo = TipoActaComision.ARegular,
        Titulo = "ACTA DE EXAMENES DE ALUMNOS A/REGULAR",
        CarreraLarga = "Tecnicatura Superior en Test",
        CuatrimestreAnio = "1/24",
        MuestraCorrespondienteCuatrimestre = true,
        Secciones =
        [
            new ActaComisionSeccion
            {
                Cabecera = new ActaComisionCabeceraDto { Cutuco = 111, CodigoMateria = "01", DescripcionMateria = "Análisis", Docente = "Prof. A" },
                Alumnos =
                [
                    new ActaAlumnoDto { CodigoAlumno = "100", Apellido = "Pérez", Nombre = "Ana", Cutuco = 111, CodigoMateria = "01" },
                    new ActaAlumnoDto { CodigoAlumno = "101", Apellido = "Gómez", Nombre = "Luis", Cutuco = 111, CodigoMateria = "01" },
                ],
            },
            new ActaComisionSeccion
            {
                Cabecera = new ActaComisionCabeceraDto { Cutuco = 222, CodigoMateria = "02", DescripcionMateria = "Lógica", Docente = null },
                Alumnos = [],
            },
        ],
    };

    private static ActaMesaModel ModeloMesa() => new()
    {
        Titulo = "ACTA DE EXAMEN FINAL",
        CarreraLarga = "Tecnicatura Superior en Test",
        Mesa = 42,
        TipoExamen = "FINAL",
        Cabecera = new ActaMesaCabeceraDto { Docente = "Titular - Vocal 1 - Vocal 2", Cutuco = 311, DescripcionMateria = "Física", Dia = 5, Mes = 7, Anio = 2026, CuatrimestreMateria = 3 },
        Alumnos =
        [
            new ActaAlumnoDto { CodigoAlumno = "200", Apellido = "Díaz", Nombre = "Sol", PermisoExamen = 7 },
            new ActaAlumnoDto { CodigoAlumno = "201", Apellido = "Ruiz", Nombre = "Juan", PermisoExamen = 8 },
        ],
    };

    private static readonly byte[] FirmaPdf = [0x25, 0x50, 0x44, 0x46];          // "%PDF"
    private static readonly byte[] FirmaZip = [0x50, 0x4B, 0x03, 0x04];          // "PK.." (xlsx es un zip)

    [Fact]
    public void ActaComision_Pdf_ProduceUnPdf()
    {
        var pdf = new ActaPdfService().GenerarActaComision(ModeloComision());

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }

    [Fact]
    public void ActaMesa_Pdf_ProduceUnPdf()
    {
        var pdf = new ActaPdfService().GenerarActaMesa(ModeloMesa());

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }

    [Fact]
    public void ActaComision_Excel_ProduceUnXlsx()
    {
        var xlsx = new ActaExcelService().GenerarActaComision(ModeloComision());

        Assert.NotEmpty(xlsx);
        Assert.Equal(FirmaZip, xlsx[..4]);
    }

    [Fact]
    public void ActaMesa_Excel_ProduceUnXlsx()
    {
        var xlsx = new ActaExcelService().GenerarActaMesa(ModeloMesa());

        Assert.NotEmpty(xlsx);
        Assert.Equal(FirmaZip, xlsx[..4]);
    }
}
