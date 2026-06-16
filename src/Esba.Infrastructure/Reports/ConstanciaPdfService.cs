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
    private const string ColorPrimario = "#1E40AF";

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

        var logo = CargarLogo();

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(2.5f, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                if (model.IncluirMembrete)
                {
                    pagina.Header().Element(c => Membrete(c, model, logo));
                }

                pagina.Content().PaddingTop(model.IncluirMembrete ? 12 : 0).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().AlignCenter().Text(model.Titulo).FontSize(13).Bold().FontColor(ColorPrimario);

                    col.Item().PaddingTop(8).Text("La Dirección del Instituto:");
                    col.Item().Text(model.Parrafo).Justify();

                    col.Item().PaddingTop(4).Text("DATOS CORRESPONDIENTES:").Bold().Underline();
                    col.Item().Text(model.MateriasQueAdeuda).FontSize(9);
                    if (!string.IsNullOrWhiteSpace(model.IdiomaLinea))
                    {
                        col.Item().Text(model.IdiomaLinea).FontSize(9);
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

                    col.Item().PaddingTop(36).Element(c => Firmas(c, model));
                });
            });
        });

        return documento.GeneratePdf();
    }

    private void Membrete(IContainer contenedor, ConstanciaAlumnoModel model, byte[]? logo)
    {
        var nombre = PrimeroNoVacio(model.Instituto, _institucion.Nombre);
        var caracteristica = PrimeroNoVacio(model.Caracteristica, _institucion.Caracteristica);

        contenedor.BorderBottom(1).BorderColor(ColorPrimario).PaddingBottom(6).Row(row =>
        {
            if (logo is not null)
            {
                row.ConstantItem(2, Unit.Centimetre).Image(logo).FitArea();
                row.Spacing(10);
            }

            row.RelativeItem().Column(col =>
            {
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    col.Item().Text(nombre).FontSize(13).Bold().FontColor(ColorPrimario);
                }

                if (!string.IsNullOrWhiteSpace(caracteristica))
                {
                    col.Item().Text(caracteristica).FontSize(9);
                }

                if (!string.IsNullOrWhiteSpace(_institucion.Direccion))
                {
                    col.Item().Text(_institucion.Direccion).FontSize(9).FontColor(Colors.Grey.Darken1);
                }
            });
        });
    }

    private static void Firmas(IContainer contenedor, ConstanciaAlumnoModel model)
    {
        contenedor.Row(row =>
        {
            row.RelativeItem().AlignCenter().Column(col =>
            {
                col.Item().AlignCenter().Text(model.Secretaria ?? string.Empty).Bold();
                col.Item().AlignCenter().Text("Secretaria");
            });

            row.RelativeItem().AlignCenter().Text("SELLO").FontColor(Colors.Grey.Medium);

            row.RelativeItem().AlignCenter().Column(col =>
            {
                col.Item().AlignCenter().Text(model.Rector ?? string.Empty).Bold();
                col.Item().AlignCenter().Text("Rectora");
            });
        });
    }

    private byte[]? CargarLogo()
    {
        var path = _institucion.LogoPath;
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    private static string PrimeroNoVacio(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) ? a!.Trim() : (b ?? string.Empty).Trim();
}
