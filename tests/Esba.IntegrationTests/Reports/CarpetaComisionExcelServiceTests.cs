using ClosedXML.Excel;
using Esba.Application.DTOs.Asistencias;
using Esba.Domain.Asistencias;
using Esba.Infrastructure.Excel;

namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Tests del export Excel de las carpetas por comisión (sucesor del BtnExcel de
/// lstNotasyPractico.pas): verificamos hoja por comisión, encabezados según el tipo
/// y la nómina con recursantes al pie, reabriendo el .xlsx con ClosedXML. No
/// necesitan base de datos (sin el trait Integration).
/// </summary>
public class CarpetaComisionExcelServiceTests
{
    private static CarpetaComisionModel Modelo(TipoCarpetaComision tipo) => new()
    {
        Tipo = tipo,
        CarreraLarga = "Bachillerato de Adultos",
        CuatrimestreAnio = "1/24",
        FechaEmision = new DateOnly(2026, 7, 15),
        Secciones =
        [
            new CarpetaComisionSeccion
            {
                Cabecera = new CarpetaComisionCabeceraDto
                {
                    Cutuco = 111, CodigoMateria = "01", DescripcionMateria = "Contabilidad",
                    Docente = "RODRIGUEZ, LUIS", TitularSuplente = "T",
                },
                Cursando =
                [
                    new CarpetaComisionAlumnoDto { CodigoAlumno = "100", Apellido = "Pérez", Nombre = "Ana", Condicion = "CURSANDO", Cutuco = 111, CodigoMateria = "01" },
                    new CarpetaComisionAlumnoDto { CodigoAlumno = "101", Apellido = "Gómez", Nombre = "Luis", Condicion = "CURSANDO", Cutuco = 111, CodigoMateria = "01" },
                ],
                Recursantes =
                [
                    new CarpetaComisionAlumnoDto { CodigoAlumno = "102", Apellido = "Ruiz", Nombre = "Eva", Condicion = "RECURSANDO", Cutuco = 111, CodigoMateria = "01" },
                ],
            },
            new CarpetaComisionSeccion
            {
                Cabecera = new CarpetaComisionCabeceraDto
                {
                    Cutuco = 222, CodigoMateria = "02", DescripcionMateria = "Psicología",
                    Docente = null, TitularSuplente = null,
                },
                Cursando = [],
                Recursantes = [],
            },
        ],
    };

    [Fact]
    public void TrabajosPracticos_Excel_UnaHojaPorComision_ConTpsYCondicion()
    {
        var bytes = new CarpetaComisionExcelService().GenerarCarpeta(Modelo(TipoCarpetaComision.TrabajosPracticos));

        using var libro = new XLWorkbook(new MemoryStream(bytes));
        Assert.Equal(2, libro.Worksheets.Count);

        var hoja = libro.Worksheet(1);
        var textos = hoja.CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains("CARPETA DE TRABAJOS PRÁCTICOS", textos);
        Assert.Contains("TP 1", textos);
        Assert.Contains("TP 5", textos);
        Assert.Contains("Condición", textos);
        Assert.Contains("Pérez, Ana", textos);
        Assert.Contains("RECURSANTES", textos);
        Assert.Contains("Ruiz, Eva", textos);
    }

    [Fact]
    public void PlanillaProfesores_Excel_ConBimestresCalificacionYNotificado()
    {
        var bytes = new CarpetaComisionExcelService().GenerarCarpeta(Modelo(TipoCarpetaComision.PlanillaProfesores));

        using var libro = new XLWorkbook(new MemoryStream(bytes));
        var hoja = libro.Worksheet(1);
        var textos = hoja.CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains("PLANILLA DE CALIFICACIONES", textos);
        Assert.Contains("1er. Bimestre", textos);
        Assert.Contains("2do Bimestre", textos);
        Assert.Contains("Calificación", textos);
        Assert.Contains("Final", textos);
        Assert.Contains("Recup.", textos);
        Assert.Contains("Def.", textos);
        Assert.Contains("Notificado", textos);
        Assert.Contains("Prom.", textos);
        Assert.Contains("Gómez, Luis", textos);
    }

    [Fact]
    public void Asistencia_Excel_NoSoportada_Arroja()
    {
        Assert.Throws<ArgumentException>(() =>
            new CarpetaComisionExcelService().GenerarCarpeta(Modelo(TipoCarpetaComision.Asistencia)));
    }
}
