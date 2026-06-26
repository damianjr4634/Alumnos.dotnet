using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Piezas de maqueta compartidas por los reportes de constancia (papel membretado y
/// firmas). El papel membretado (JPG A4 "membrete_con_direccion.jpg") se compone como
/// fondo de página, igual en todas las constancias. Las firmas van como texto (A4),
/// según la decisión de normalización del hito 9.2 (firmas-imagen diferidas a hito 12).
/// </summary>
internal static class ReporteConstanciaLayout
{
    public const string ColorPrimario = "#1E40AF";

    /// <summary>
    /// Lee el JPG del papel membretado (A4). Resuelve rutas relativas contra el directorio
    /// de ejecución. null si no está configurado o el archivo no existe (la constancia sale
    /// sin fondo, para impresión sobre papel preimpreso).
    /// </summary>
    public static byte[]? CargarFondo(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var ruta = Path.IsPathRooted(path) ? path : Path.Combine(Directory.GetCurrentDirectory(), path);
        return File.Exists(ruta) ? File.ReadAllBytes(ruta) : null;
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
}
