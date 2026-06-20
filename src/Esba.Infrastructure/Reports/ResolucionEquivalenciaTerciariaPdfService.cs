using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de la resolución de equivalencia terciaria (formato nuevo de
/// lst_impresion_equivalencia_terc.pas). Dibuja el papel membretado (JPG) de fondo en
/// cada hoja y deja que QuestPDF pagine el cuerpo VISTO/CONSIDERANDO/RESUELVE, en vez del
/// manejo manual de páginas y los <c>TextOut</c> posicionados del legacy.
/// </summary>
public sealed class ResolucionEquivalenciaTerciariaPdfService : IResolucionEquivalenciaReportService
{
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;

    private readonly InstitucionSettings _institucion;

    static ResolucionEquivalenciaTerciariaPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ResolucionEquivalenciaTerciariaPdfService(IOptions<InstitucionSettings> institucion)
    {
        _institucion = institucion.Value;
    }

    public byte[] GenerarResolucionTerciaria(ResolucionEquivalenciaTerciariaModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var membrete = model.IncluirMembrete ? CargarMembrete(_institucion.MembreteResolucionPath) : null;
        var fecha = $"Buenos Aires, {model.Fecha.Day} de {TextoCastellano.MesEnLetras(model.Fecha.Month)} de {model.Fecha.Year}";

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
                    col.Spacing(8);

                    col.Item().AlignRight().Text(fecha);
                    col.Item().AlignRight().Text($"Acta Interna: {model.ActasInternas}");

                    Seccion(col, "VISTO:", model.TextoVisto);
                    Seccion(col, "CONSIDERANDO:", model.TextoConsiderando);

                    col.Item().PaddingTop(4).Text("RESUELVE:").Bold();
                    col.Item().Text(ResolucionEquivalenciaFormatter.ArticuloPrimero);
                    foreach (var materia in model.Materias)
                    {
                        col.Item().PaddingLeft(12).Text(materia).Justify();
                    }

                    col.Item().PaddingTop(48).AlignRight().Column(firma =>
                    {
                        firma.Item().Text(model.Rector ?? string.Empty).Bold();
                        firma.Item().Text("Rector/a").FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static void Seccion(ColumnDescriptor col, string titulo, string cuerpo)
    {
        col.Item().PaddingTop(4).Text(titulo).Bold().FontColor(ColorPrimario);
        col.Item().Text(cuerpo).Justify();
    }

    // Lee el JPG del membrete; resuelve rutas relativas contra el directorio de ejecución
    // (en el Web, el content root que contiene wwwroot). null si no está configurado o no existe.
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
