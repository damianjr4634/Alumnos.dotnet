using Esba.Application.Abstractions;
using Esba.Application.Common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Exportación tabular a PDF con QuestPDF (reemplaza Gnostice eDocEngine + GDI
/// manual, §3.5). El PDF se devuelve como byte[] y la UI lo ofrece para
/// descargar/previsualizar. Servicio sin estado.
/// </summary>
public sealed class QuestPdfExportService : IPdfExportService
{
    static QuestPdfExportService()
    {
        // QuestPDF exige fijar la licencia antes de generar; Community es gratuita
        // para esta escala (https://www.questpdf.com/license/).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportarTabla<T>(
        IReadOnlyList<T> filas,
        IReadOnlyList<ColumnaExportable<T>> columnas,
        string titulo)
    {
        ArgumentNullException.ThrowIfNull(filas);
        ArgumentNullException.ThrowIfNull(columnas);

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4.Landscape());
                pagina.Margin(1.5f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                pagina.Header().Column(col =>
                {
                    col.Item().Text(titulo).FontSize(14).SemiBold().FontColor("#1E40AF");
                    col.Item().Text($"{filas.Count} registro(s)").FontSize(8).FontColor(Colors.Grey.Medium);
                });

                pagina.Content().PaddingVertical(8).Table(tabla =>
                {
                    tabla.ColumnsDefinition(def =>
                    {
                        foreach (var _ in columnas)
                        {
                            def.RelativeColumn();
                        }
                    });

                    foreach (var columna in columnas)
                    {
                        tabla.Cell().Element(EstiloEncabezado).Text(columna.Titulo)
                            .FontColor(Colors.White).SemiBold();
                    }

                    foreach (var fila in filas)
                    {
                        foreach (var columna in columnas)
                        {
                            var celda = tabla.Cell().Element(EstiloCelda);
                            celda = columna.AlinearDerecha ? celda.AlignRight() : celda.AlignLeft();
                            celda.Text(columna.Formatear(fila));
                        }
                    }
                });

                pagina.Footer().AlignRight().Text(texto =>
                {
                    texto.Span("Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static IContainer EstiloEncabezado(IContainer contenedor) =>
        contenedor.Background("#1E40AF").PaddingVertical(4).PaddingHorizontal(5);

    private static IContainer EstiloCelda(IContainer contenedor) =>
        contenedor.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(5);
}
