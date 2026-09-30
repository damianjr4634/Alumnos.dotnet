using ClosedXML.Excel;
using Esba.Application.DTOs.Ministerio;
using Esba.Infrastructure.Excel;

namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Tests del export Excel del padrón de comisiones al Ministerio (sucesor del
/// BitBtn1Click de ComisionesAlMinisterio.pas): títulos de columna del layout
/// terciario y secundario, "Juntas" en una hoja y "Separadas" en una hoja por
/// comisión nombrada CARRE-CUTUCO; se verifica reabriendo el .xlsx con ClosedXML.
/// No necesitan base de datos (sin el trait Integration).
/// </summary>
public class PadronMinisterioExcelServiceTests
{
    private static FilaPadronMinisterioDto Fila(short cutuco, string apellido, string? condicion = "R") => new()
    {
        Cutuco = cutuco,
        NombreHoja = $"TEC-{cutuco}",
        Resolucion = "1234/20",
        Orientacion = "TC",
        TipoCarrera = "G",
        Modalidad = "P",
        Turno = "TM",
        AnioEstudio = 1,
        Cuatrimestre = 1,
        Division = "A",
        Apellido = apellido,
        Nombre = "Ana",
        TipoDocumento = "DNI",
        NumeroDocumento = "12.345.678",
        Genero = "MUJER",
        FechaNacimiento = new DateOnly(2005, 3, 15),
        PaisNacimiento = "ARGENTINA",
        LugarNacimiento = "CABA",
        Domicilio = "Calle 1",
        CodigoPostal = 1414,
        Localidad = "CABA",
        Condicion = condicion,
        TituloIngreso = "Colegio Bachiller",
        ApellidoTutor = "Tutor",
        NombreTutor = "Nombre Tutor",
        TipoDocumentoTutor = "DNI",
        DniTutor = 99999999,
    };

    private static PadronMinisterioModel Modelo(bool terciaria, bool separadas, params FilaPadronMinisterioDto[] filas) => new()
    {
        CodigoCarrera = "TEC",
        EsTerciaria = terciaria,
        HojasSeparadas = separadas,
        Filas = filas,
    };

    private static XLWorkbook Abrir(byte[] contenido) => new(new MemoryStream(contenido));

    [Fact]
    public void Terciaria_Juntas_UnaHojaConLas24ColumnasDelLayoutTerciario()
    {
        var xlsx = new PadronMinisterioExcelService().GenerarPadron(
            Modelo(terciaria: true, separadas: false, Fila(111, "Pérez"), Fila(112, "Gómez", "RC")));

        using var libro = Abrir(xlsx);
        var hoja = Assert.Single(libro.Worksheets);
        Assert.Equal("Comisiones", hoja.Name);

        var titulos = hoja.Row(1).CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Equal(24, titulos.Count);
        Assert.Equal("NÚMERO DE RESOLUCION DEL PLAN", titulos[0]);
        Assert.Equal("ORIENTACION DE LA CARRERA (TS=TECNICO SUPERIOR)", titulos[1]);
        Assert.Equal("CONDICION (R=REGULAR, C=CONDICIONAL, RC=RECURSANTE)", titulos[22]);
        Assert.Equal("TITULO CON EL QUE INGRESA", titulos[23]);
        Assert.DoesNotContain("APELLIDO ADULTO RESPONSABLE", titulos);

        // Dos filas de datos debajo del encabezado, con la fecha como fecha real.
        Assert.Equal("Pérez", hoja.Cell(2, 9).GetString());
        Assert.Equal("Gómez", hoja.Cell(3, 9).GetString());
        Assert.Equal("RC", hoja.Cell(3, 23).GetString());
        Assert.Equal(new DateTime(2005, 3, 15), hoja.Cell(2, 14).GetDateTime());
        Assert.Equal(1414, hoja.Cell(2, 19).GetValue<int>());
        Assert.True(hoja.Cell(2, 18).IsEmpty());   // PISO Y DEPTO. siempre en blanco
    }

    [Fact]
    public void Secundaria_Juntas_UnaHojaConLas26ColumnasDelLayoutSecundario()
    {
        var xlsx = new PadronMinisterioExcelService().GenerarPadron(
            Modelo(terciaria: false, separadas: false, Fila(111, "Pérez")));

        using var libro = Abrir(xlsx);
        var hoja = Assert.Single(libro.Worksheets);

        var titulos = hoja.Row(1).CellsUsed().Select(c => c.GetString()).ToList();
        Assert.Equal(26, titulos.Count);
        Assert.Equal("MODALIDAD (C=COMÚN, A=ADULTOS)", titulos[1]);
        Assert.Equal("CON PPI SIGNIFICATIVAS (X)", titulos[13]);
        Assert.Equal("APELLIDO ADULTO RESPONSABLE", titulos[22]);
        Assert.Equal("NRO. DOCUMENTO DEL ADULTO RESPONSABLE", titulos[25]);
        Assert.DoesNotContain("TITULO CON EL QUE INGRESA", titulos);

        Assert.Equal("Tutor", hoja.Cell(2, 23).GetString());
        Assert.Equal(99999999, hoja.Cell(2, 26).GetValue<int>());
    }

    [Fact]
    public void Separadas_UnaHojaPorComisionNombradaCarreraCutuco()
    {
        var xlsx = new PadronMinisterioExcelService().GenerarPadron(
            Modelo(terciaria: true, separadas: true, Fila(111, "Pérez"), Fila(111, "Ruiz"), Fila(112, "Gómez")));

        using var libro = Abrir(xlsx);
        Assert.Equal(2, libro.Worksheets.Count);
        Assert.Equal("TEC-111", libro.Worksheet(1).Name);
        Assert.Equal("TEC-112", libro.Worksheet(2).Name);

        // Cada hoja lleva su propio encabezado y solo sus alumnos.
        Assert.Equal("NÚMERO DE RESOLUCION DEL PLAN", libro.Worksheet(2).Cell(1, 1).GetString());
        Assert.Equal(2, libro.Worksheet(1).RowsUsed().Count() - 1);
        Assert.Equal(1, libro.Worksheet(2).RowsUsed().Count() - 1);
        Assert.Equal("Gómez", libro.Worksheet(2).Cell(2, 9).GetString());
    }
}
