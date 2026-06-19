using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Piezas de maqueta compartidas por los reportes de constancia (membrete, firmas y
/// carga de logo). Evita duplicar el encabezado/pie entre la constancia de texto
/// (<see cref="ConstanciaPdfService"/>) y la constancia tabular de materias
/// (<see cref="ConstanciaAnaliticoPdfService"/>). Las firmas van como texto (A4),
/// según la decisión de normalización del hito 9.2 (firmas-imagen diferidas a hito 12).
/// </summary>
internal static class ReporteConstanciaLayout
{
    public const string ColorPrimario = "#1E40AF";

    /// <summary>Membrete institucional: logo opcional + nombre/característica/dirección.</summary>
    public static void Membrete(IContainer contenedor, InstitucionSettings institucion, string? instituto, string? caracteristica, byte[]? logo)
    {
        var nombre = PrimeroNoVacio(instituto, institucion.Nombre);
        var caract = PrimeroNoVacio(caracteristica, institucion.Caracteristica);

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

                if (!string.IsNullOrWhiteSpace(caract))
                {
                    col.Item().Text(caract).FontSize(9);
                }

                if (!string.IsNullOrWhiteSpace(institucion.Direccion))
                {
                    col.Item().Text(institucion.Direccion).FontSize(9).FontColor(Colors.Grey.Darken1);
                }
            });
        });
    }

    /// <summary>Firmas de las autoridades como texto (nombre + cargo) con el sello al medio.</summary>
    public static void Firmas(IContainer contenedor, string? secretaria, string? rector)
    {
        contenedor.Row(row =>
        {
            row.RelativeItem().AlignCenter().Column(col =>
            {
                col.Item().AlignCenter().Text(secretaria ?? string.Empty).Bold();
                col.Item().AlignCenter().Text("Secretaria");
            });

            row.RelativeItem().AlignCenter().Text("SELLO").FontColor(Colors.Grey.Medium);

            row.RelativeItem().AlignCenter().Column(col =>
            {
                col.Item().AlignCenter().Text(rector ?? string.Empty).Bold();
                col.Item().AlignCenter().Text("Rectora");
            });
        });
    }

    public static byte[]? CargarLogo(InstitucionSettings institucion)
    {
        var path = institucion.LogoPath;
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path) ? File.ReadAllBytes(path) : null;
    }

    private static string PrimeroNoVacio(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) ? a!.Trim() : (b ?? string.Empty).Trim();
}
