using System.IO.Compression;
using ClosedXML.Excel;
using Esba.Application.DTOs.Asistencias;
using Esba.Domain.Asistencias;
using Esba.Infrastructure.Excel;

namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Tests del export Excel de las carpetas por comisión (sucesor del BtnExcel de
/// lstNotasyPractico.pas): un archivo por comisión/materia como el legacy — .xlsx
/// directo si es una sola, .zip si son varias —, con encabezados según el tipo y la
/// nómina con recursantes al pie; se verifica reabriendo el .xlsx con ClosedXML.
/// No necesitan base de datos (sin el trait Integration).
/// </summary>
public class CarpetaComisionExcelServiceTests
{
    private static CarpetaComisionSeccion Seccion1 => new()
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
    };

    private static CarpetaComisionSeccion Seccion2 => new()
    {
        Cabecera = new CarpetaComisionCabeceraDto
        {
            Cutuco = 222, CodigoMateria = "02", DescripcionMateria = "Psicología",
            Docente = null, TitularSuplente = null,
        },
        Cursando = [],
        Recursantes = [],
    };

    private static CarpetaComisionModel Modelo(TipoCarpetaComision tipo, params CarpetaComisionSeccion[] secciones) => new()
    {
        Tipo = tipo,
        CarreraLarga = "Bachillerato de Adultos",
        CuatrimestreAnio = "1/24",
        FechaEmision = new DateOnly(2026, 7, 15),
        Secciones = secciones,
    };

    [Fact]
    public void TrabajosPracticos_VariasComisiones_GeneraUnZipConUnXlsxPorComision()
    {
        var resultado = new CarpetaComisionExcelService().GenerarCarpeta(
            Modelo(TipoCarpetaComision.TrabajosPracticos, Seccion1, Seccion2));

        Assert.True(resultado.EsZip);
        Assert.Equal("trabajos_practicos.zip", resultado.NombreArchivo);

        using var zip = new ZipArchive(new MemoryStream(resultado.Contenido), ZipArchiveMode.Read);
        Assert.Equal(2, zip.Entries.Count);
        Assert.Equal("TP_111_Contabilidad.xlsx", zip.Entries[0].Name);
        Assert.Equal("TP_222_Psicología.xlsx", zip.Entries[1].Name);

        using var memoria = new MemoryStream();
        zip.Entries[0].Open().CopyTo(memoria);
        memoria.Position = 0;
        using var libro = new XLWorkbook(memoria);
        var textos = libro.Worksheet(1).CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Contains("CARPETA DE TRABAJOS PRÁCTICOS", textos);
        Assert.Contains("TP 1", textos);
        Assert.Contains("TP 5", textos);
        Assert.Contains("Condición", textos);
        Assert.Contains("Pérez, Ana", textos);
        Assert.Contains("RECURSANTES", textos);
        Assert.Contains("Ruiz, Eva", textos);
    }

    [Fact]
    public void PlanillaProfesores_UnaComision_GeneraElXlsxDirecto_ConBimestresCalificacionYNotificado()
    {
        var resultado = new CarpetaComisionExcelService().GenerarCarpeta(
            Modelo(TipoCarpetaComision.PlanillaProfesores, Seccion1));

        Assert.False(resultado.EsZip);
        Assert.Equal("Notas_111_Contabilidad.xlsx", resultado.NombreArchivo);

        using var libro = new XLWorkbook(new MemoryStream(resultado.Contenido));
        var textos = libro.Worksheet(1).CellsUsed().Select(c => c.GetString()).ToList();
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
            new CarpetaComisionExcelService().GenerarCarpeta(Modelo(TipoCarpetaComision.Asistencia, Seccion1)));
    }
}
