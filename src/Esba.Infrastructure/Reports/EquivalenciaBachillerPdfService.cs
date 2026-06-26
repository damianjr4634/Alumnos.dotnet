using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de la equivalencia bachiller, sucesor de los <c>TextOut</c>
/// posicionados sobre el membrete WMF de lst_impresion_equivalencia_bac.pas. En vez de
/// calcar coordenadas, reflowamos el contenido a A4 reusando el membrete compartido
/// (<see cref="ReporteConstanciaLayout"/>, decisión hito 9.2). Solo maqueta: los textos
/// llegan resueltos en <see cref="EquivalenciaBachillerModel"/>.
/// </summary>
public sealed class EquivalenciaBachillerPdfService : IEquivalenciaBachillerReportService
{
    private const string Titulo = "EQUIVALENCIA — BACHILLERATO";
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;

    private readonly InstitucionSettings _institucion;

    static EquivalenciaBachillerPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public EquivalenciaBachillerPdfService(IOptions<InstitucionSettings> institucion)
    {
        _institucion = institucion.Value;
    }

    public byte[] GenerarEquivalenciaBachiller(EquivalenciaBachillerModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var membrete = ReporteConstanciaLayout.CargarFondo(_institucion.MembreteConstanciaPath);
        var fecha = $"Buenos Aires, {model.Fecha.Day} de {TextoCastellano.MesEnLetras(model.Fecha.Month)} de {model.Fecha.Year}";

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                // El membrete trae su propio encabezado/pie: el cuerpo arranca más abajo.
                pagina.MarginVertical(membrete is not null ? 4.5f : 2.5f, Unit.Centimetre);
                pagina.MarginHorizontal(2.5f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                if (membrete is not null)
                {
                    pagina.Background().Image(membrete).FitArea();
                }

                pagina.Content().Column(col =>
                {
                    col.Spacing(6);

                    col.Item().AlignRight().Text(fecha);
                    col.Item().AlignRight().Text($"Resolución Interna N°: {model.ResolucionInterna}");

                    col.Item().PaddingTop(8).Text(texto =>
                    {
                        texto.Span($"{model.NombreAlumno} ").Bold();
                        texto.Span(model.CodigoAlumno);
                    });
                    col.Item().Text($"{model.NombreCarrera} — Ciclo Lectivo: {model.CicloLectivo}");
                    col.Item().Text(model.TextoVista).Justify();

                    col.Item().PaddingTop(6).Element(c => Cuerpo(c, model.Lineas));

                    if (model.MostrarNotaAdReferendum)
                    {
                        col.Item().PaddingTop(6).Text(EquivalenciaBachillerFormatter.NotaAdReferendum).FontSize(8).Italic();
                    }

                    col.Item().PaddingTop(24).AlignCenter().Text(model.NombreCarrera).Bold().FontColor(ColorPrimario);
                });
            });
        });

        return documento.GeneratePdf();
    }

    // Listado de materias a dos columnas (COLUMNA1/COLUMNA2 del SP), una celda por columna
    // con la fuente del documento. El texto llega formateado por el SP (incluidas las
    // descripciones de MATERIAS tal como están cargadas, con acentos inconsistentes del
    // dato legacy: se respetan por fidelidad). El relleno de asteriscos se omite.
    private static void Cuerpo(IContainer contenedor, IReadOnlyList<LineaEquivalenciaBachillerDto> lineas)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.RelativeColumn();
                columnas.RelativeColumn();
            });

            foreach (var linea in lineas)
            {
                Celda(tabla, linea.Columna1);
                Celda(tabla, EsRelleno(linea.Columna2) ? null : linea.Columna2);
            }
        });
    }

    private static void Celda(TableDescriptor tabla, string? texto) =>
        tabla.Cell().PaddingVertical(1).Text(texto ?? string.Empty);

    private static bool EsRelleno(string? texto)
    {
        var t = (texto ?? string.Empty).Trim();
        return t.Length > 0 && t.All(c => c == '*');
    }
}
