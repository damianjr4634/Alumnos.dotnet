using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Esba.Infrastructure.Reports;

/// <summary>
/// Piezas de maqueta compartidas por los reportes sobre papel membretado: el fondo
/// (JPG A4 u Oficio), el sello institucional y las firmas de las autoridades. Regla
/// (usuario, 2026-10-01): toda impresión con membrete cierra, en su última hoja, con el
/// sello al lado de las firmas; la imagen de la firma de la rectora o de la secretaria
/// sale únicamente cuando su nombre está impreso. Sin archivo configurado, cada pieza
/// degrada a texto (o se omite), para impresión sobre papel preimpreso.
/// </summary>
internal static class ReporteConstanciaLayout
{
    public const string ColorPrimario = "#1E40AF";

    private const float AltoFirmaCm = 2f;
    private const float AnchoFirmaCm = 5f;
    private const float AltoSelloCm = 3.2f;

    /// <summary>
    /// Lee un JPG/PNG (membrete, sello o firma). Resuelve rutas relativas contra el
    /// directorio de ejecución. null si no está configurado o el archivo no existe.
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

    /// <summary>Imágenes institucionales del pie: sello y firmas (cualquiera puede faltar).</summary>
    public sealed record ImagenesAutoridades(byte[]? Sello, byte[]? FirmaRector, byte[]? FirmaSecretaria, byte[]? FirmaDirectorEstudios)
    {
        public static readonly ImagenesAutoridades Ninguna = new(null, null, null, null);

        public static ImagenesAutoridades Cargar(InstitucionSettings institucion)
        {
            ArgumentNullException.ThrowIfNull(institucion);
            return new ImagenesAutoridades(
                CargarFondo(institucion.SelloPath),
                CargarFondo(institucion.FirmaRectorPath),
                CargarFondo(institucion.FirmaSecretariaPath),
                CargarFondo(institucion.FirmaDirectorEstudiosPath));
        }
    }

    /// <summary>
    /// Pie de firmas de las constancias: secretaria a la izquierda, sello al medio,
    /// rector/a a la derecha. Cada firma lleva su imagen arriba del nombre solo si el
    /// nombre viene informado y la imagen existe.
    /// </summary>
    public static void Firmas(
        IContainer contenedor, string? secretaria, string? rector, ImagenesAutoridades imagenes, string cargoRector = "Rectora")
    {
        ArgumentNullException.ThrowIfNull(imagenes);

        contenedor.Row(row =>
        {
            row.RelativeItem().Element(c => Firma(c, secretaria, "Secretaria", imagenes.FirmaSecretaria));
            row.RelativeItem().AlignCenter().AlignBottom().Element(c => Sello(c, imagenes));
            row.RelativeItem().Element(c => Firma(c, rector, cargoRector, imagenes.FirmaRector));
        });
    }

    /// <summary>
    /// Pie con una sola autoridad firmante a la derecha (resoluciones, citaciones) y el
    /// sello a su izquierda.
    /// </summary>
    public static void FirmaUnica(
        IContainer contenedor, string? nombre, string cargo, byte[]? firma, ImagenesAutoridades imagenes)
    {
        ArgumentNullException.ThrowIfNull(imagenes);

        contenedor.Row(row =>
        {
            row.RelativeItem();
            row.RelativeItem().AlignCenter().AlignBottom().Element(c => Sello(c, imagenes));
            row.RelativeItem().Element(c => Firma(c, nombre, cargo, firma));
        });
    }

    /// <summary>Sello solo (impresiones con membrete que no llevan firmas): centrado al pie.</summary>
    public static void SoloSello(IContainer contenedor, ImagenesAutoridades imagenes)
    {
        ArgumentNullException.ThrowIfNull(imagenes);
        if (imagenes.Sello is null)
        {
            return;
        }

        contenedor.AlignCenter().Element(c => Sello(c, imagenes));
    }

    /// <summary>Imagen de firma (si corresponde) sobre el nombre en negrita y el cargo debajo.</summary>
    private static void Firma(IContainer contenedor, string? nombre, string cargo, byte[]? firma)
    {
        var tieneNombre = !string.IsNullOrWhiteSpace(nombre);

        contenedor.AlignCenter().Column(col =>
        {
            if (tieneNombre && firma is not null)
            {
                col.Item().AlignCenter().Width(AnchoFirmaCm, Unit.Centimetre).Height(AltoFirmaCm, Unit.Centimetre)
                    .Image(firma).FitArea();
            }
            else
            {
                // Espacio para la firma manuscrita cuando no hay imagen.
                col.Item().Height(AltoFirmaCm, Unit.Centimetre);
            }

            col.Item().AlignCenter().Text(nombre?.Trim() ?? string.Empty).Bold();
            col.Item().AlignCenter().Text(cargo);
        });
    }

    private static void Sello(IContainer contenedor, ImagenesAutoridades imagenes)
    {
        if (imagenes.Sello is not null)
        {
            contenedor.Height(AltoSelloCm, Unit.Centimetre).Image(imagenes.Sello).FitHeight();
        }
        else
        {
            contenedor.Text("SELLO").FontColor(Colors.Grey.Medium);
        }
    }
}
