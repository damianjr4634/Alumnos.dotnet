using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Reporte QuestPDF de la constancia de alumno (sucesor del dibujo GDI de
/// constanciaalumnos2.pas sobre TGmPreview). Solo maqueta: el contenido ya viene
/// resuelto en <see cref="ConstanciaAlumnoModel"/> (§2.1). Primer reporte con
/// formato propio del sistema; sienta el patrón para los demás.
/// </summary>
public sealed class ConstanciaPdfService : IConstanciaReportService
{
    private const string ColorPrimario = ReporteConstanciaLayout.ColorPrimario;

    private readonly InstitucionSettings _institucion;

    static ConstanciaPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public ConstanciaPdfService(IOptions<InstitucionSettings> institucion)
    {
        _institucion = institucion.Value;
    }

    public byte[] GenerarConstanciaAlumno(ConstanciaAlumnoModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var membrete = ReporteConstanciaLayout.CargarFondo(_institucion.MembreteConstanciaPath);

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

                    col.Item().AlignCenter().Text(model.Titulo).FontSize(13).Bold().FontColor(ColorPrimario);

                    col.Item().PaddingTop(8).Text("La Dirección del Instituto:");
                    col.Item().Text(model.Parrafo).Justify();

                    if (!string.IsNullOrWhiteSpace(model.MateriasQueAdeuda))
                    {
                        col.Item().PaddingTop(4).Text("DATOS CORRESPONDIENTES:").Bold().Underline();
                        col.Item().Text(model.MateriasQueAdeuda).FontSize(9);
                        if (!string.IsNullOrWhiteSpace(model.IdiomaLinea))
                        {
                            col.Item().Text(model.IdiomaLinea).FontSize(9);
                        }
                    }

                    col.Item().PaddingTop(8).Text(model.ParrafoCierre).Justify();

                    col.Item().PaddingTop(6).Column(notas =>
                    {
                        notas.Spacing(2);
                        foreach (var nota in model.NotasLegales)
                        {
                            notas.Item().Text(nota).FontSize(8);
                        }
                    });

                    col.Item().PaddingTop(36).Element(c =>
                        ReporteConstanciaLayout.Firmas(c, model.Secretaria, model.Rector));
                });
            });
        });

        return documento.GeneratePdf();
    }
}
