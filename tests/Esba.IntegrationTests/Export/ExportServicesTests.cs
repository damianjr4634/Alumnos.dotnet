using ClosedXML.Excel;
using Esba.Application.Common;
using Esba.Infrastructure.Excel;
using Esba.Infrastructure.Reports;
using Xunit;

namespace Esba.IntegrationTests.Export;

/// <summary>
/// Tests de los servicios genéricos de exportación de EsbaListView (hito 5).
/// Son puros (no tocan la base): viven en este proyecto solo porque es el único
/// que referencia Infrastructure; no llevan el trait Integration y corren en el
/// ciclo rápido.
/// </summary>
public class ExportServicesTests
{
    private sealed record Fila(string Codigo, string Nombre, bool Anual, short Orden, DateOnly Alta);

    private static readonly IReadOnlyList<Fila> Filas =
    [
        new("01", "Matemática I", true, 1, new DateOnly(2026, 3, 15)),
        new("02", "Lengua", false, 2, new DateOnly(2026, 8, 1)),
    ];

    private static readonly IReadOnlyList<ColumnaExportable<Fila>> Columnas =
    [
        new() { Titulo = "Código", Valor = f => f.Codigo },
        new() { Titulo = "Nombre", Valor = f => f.Nombre },
        new() { Titulo = "Anual", Valor = f => f.Anual },
        new() { Titulo = "Orden", Valor = f => f.Orden, AlinearDerecha = true },
        new() { Titulo = "Alta", Valor = f => f.Alta, Formato = "dd/MM/yyyy" },
    ];

    [Fact]
    public void Excel_VuelcaEncabezadosYFilas()
    {
        var servicio = new ClosedXmlExportService();

        var bytes = servicio.Exportar(Filas, Columnas, "Materias");

        Assert.NotEmpty(bytes);
        using var libro = new XLWorkbook(new MemoryStream(bytes));
        var hoja = libro.Worksheets.First();

        // Encabezados.
        Assert.Equal("Código", hoja.Cell(1, 1).GetString());
        Assert.Equal("Alta", hoja.Cell(1, 5).GetString());

        // Primera fila de datos.
        Assert.Equal("01", hoja.Cell(2, 1).GetString());
        Assert.Equal("Matemática I", hoja.Cell(2, 2).GetString());
        Assert.Equal("Sí", hoja.Cell(2, 3).GetString());        // bool → Sí/No
        Assert.Equal("No", hoja.Cell(3, 3).GetString());
        Assert.Equal(1, hoja.Cell(2, 4).GetValue<int>());        // número tipado
        Assert.Equal(new DateTime(2026, 3, 15), hoja.Cell(2, 5).GetDateTime()); // fecha tipada
    }

    [Fact]
    public void Excel_SinFilas_GeneraSoloEncabezados()
    {
        var servicio = new ClosedXmlExportService();

        var bytes = servicio.Exportar(Array.Empty<Fila>(), Columnas, "Vacío");

        using var libro = new XLWorkbook(new MemoryStream(bytes));
        var hoja = libro.Worksheets.First();
        Assert.Equal("Código", hoja.Cell(1, 1).GetString());
        Assert.True(hoja.Cell(2, 1).IsEmpty());
    }

    [Fact]
    public void Pdf_GeneraDocumentoValido()
    {
        var servicio = new QuestPdfExportService();

        var bytes = servicio.ExportarTabla(Filas, Columnas, "Materias");

        Assert.NotEmpty(bytes);
        // Firma de archivo PDF: "%PDF".
        Assert.Equal(new byte[] { 0x25, 0x50, 0x44, 0x46 }, bytes[..4]);
    }
}
