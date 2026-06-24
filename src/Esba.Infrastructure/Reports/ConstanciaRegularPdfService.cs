using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de la Constancia de Alumno Regular (sucesor de CreatePDF de
/// constanciaalumnoregular.pas). Hoja A4 con membrete_con_direccion.jpg de fondo;
/// QuestPDF maqueta el cuerpo, en vez de los TextOut posicionados del legacy.
/// </summary>
public sealed class ConstanciaRegularPdfService : IConstanciaRegularReportService
{
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;

    private readonly InstitucionSettings _institucion;

    static ConstanciaRegularPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ConstanciaRegularPdfService(IOptions<InstitucionSettings> institucion)
    {
        _institucion = institucion.Value;
    }

    public byte[] GenerarConstanciaRegular(ConstanciaRegularModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var membrete = model.IncluirMembrete ? CargarMembrete(_institucion.MembreteConstanciaRegularPath) : null;

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                // El membrete trae su propio encabezado/pie: el cuerpo arranca más abajo.
                pagina.MarginVertical(membrete is not null ? 4.5f : 2.5f, Unit.Centimetre);
                pagina.MarginHorizontal(2.5f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                if (membrete is not null)
                {
                    pagina.Background().Image(membrete).FitArea();
                }

                pagina.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().PaddingBottom(8).AlignCenter().Text(model.Titulo)
                        .FontSize(14).Bold().FontColor(ColorPrimario);

                    foreach (var parrafo in model.Cuerpo)
                    {
                        col.Item().Text(parrafo).Justify();
                    }

                    // Firmas (Secretaria izquierda, Rector/a derecha) con el sello al medio.
                    col.Item().PaddingTop(56).Row(row =>
                    {
                        row.RelativeItem().AlignCenter().Column(firma =>
                        {
                            firma.Item().AlignCenter().Text(model.Secretaria ?? string.Empty).Bold();
                            firma.Item().AlignCenter().Text("Secretaria");
                        });

                        row.RelativeItem().AlignCenter().Text("SELLO").FontColor(Colors.Grey.Medium);

                        row.RelativeItem().AlignCenter().Column(firma =>
                        {
                            firma.Item().AlignCenter().Text(model.Rector ?? string.Empty).Bold();
                            firma.Item().AlignCenter().Text("Rector/a");
                        });
                    });

                    col.Item().PaddingTop(24).Text(model.NotaLegal).FontSize(9).Italic();

                    if (!string.IsNullOrWhiteSpace(model.LineaSubvencion))
                    {
                        col.Item().Text(model.LineaSubvencion).FontSize(9);
                    }
                });
            });
        });

        return documento.GeneratePdf();
    }

    // Lee el JPG del membrete; resuelve rutas relativas contra el directorio de ejecución.
    // null si no está configurado o no existe (la constancia sale sin fondo, para preimpreso).
    private static byte[]? CargarMembrete(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var ruta = Path.IsPathRooted(path) ? path : Path.Combine(Directory.GetCurrentDirectory(), path);
        return File.Exists(ruta) ? File.ReadAllBytes(ruta) : null;
    }
}
