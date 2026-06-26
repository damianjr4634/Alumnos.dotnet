using Esba.Application.DTOs.Certificados;

namespace Esba.Web.Components.Shared;

/// <summary>
/// Estilo CSS del chip de condición de una materia, con el color que calcula
/// <c>XXX_CONSTANCIA_TERCIARIA</c> en sus campos <c>HTMLCOLOR</c> (fondo) y
/// <c>HTMLFONTCOLOR</c> (fuente). Cuando esos campos vienen vacíos —la rama ADM del
/// SP no los setea y la <c>TBL_COLOR.HTMLCODE</c> puede no estar cargada— se
/// reconstruye el color desde los <c>TColor</c> enteros de la VCL
/// (<c>COLOR</c>/<c>FONTCOLOR</c>, formato <c>0x00BBGGRR</c>).
///
/// Devuelve <c>null</c> cuando el SP no asigna un color especial (fondo blanco, el
/// estado "normal"): en ese caso la pantalla cae al chip semántico del tema
/// (<see cref="CondicionMateriaColor"/>), respetando el modo claro/oscuro (§4.5).
/// </summary>
public static class ColorCondicionMateria
{
    public static string? EstiloChip(ConstanciaMateriaDto materia)
    {
        var fondo = ColorCss(materia.HtmlColor, materia.ColorFondo);
        if (fondo is null || EsBlanco(fondo))
        {
            return null;
        }

        var fuente = ColorCss(materia.HtmlFontColor, materia.ColorFuente) ?? "#000000";
        return $"background-color:{fondo};color:{fuente};";
    }

    private static string? ColorCss(string? html, int? vcl)
    {
        if (!string.IsNullOrWhiteSpace(html))
        {
            return html.Trim();
        }

        // TColor de la VCL: entero 0x00BBGGRR (byte bajo = rojo).
        return vcl is { } c
            ? $"#{c & 0xFF:X2}{(c >> 8) & 0xFF:X2}{(c >> 16) & 0xFF:X2}"
            : null;
    }

    private static bool EsBlanco(string color) =>
        color.Equals("white", StringComparison.OrdinalIgnoreCase)
        || color.Equals("#fff", StringComparison.OrdinalIgnoreCase)
        || color.Equals("#ffffff", StringComparison.OrdinalIgnoreCase);
}
