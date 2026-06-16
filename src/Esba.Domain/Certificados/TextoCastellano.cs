namespace Esba.Domain.Certificados;

/// <summary>
/// Helpers de texto en castellano para constancias y certificados. Sucesores de
/// las funciones VCL <c>MesALetras</c> y <c>LetrasCuat</c> que el legacy usaba al
/// componer los párrafos (constanciaalumnos2.pas).
/// </summary>
/// <remarks>
/// ⚠️ Las definiciones legacy de <c>MesALetras</c>/<c>LetrasCuat</c> no están en
/// el repositorio (vivían en una unidad no provista). Se reconstruyen con los
/// nombres estándar en castellano; los meses coinciden con el CASE de
/// <c>XXX_PARRAFO_CONSTANCIA</c>. // TODO-confirmar contra el binario legacy.
/// </remarks>
public static class TextoCastellano
{
    private static readonly string[] Meses =
    [
        "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre",
    ];

    // Ordinales masculinos apocopados ("DEL PRIMER CUAT.", "DEL SEGUNDO CUAT.").
    private static readonly string[] Ordinales =
    [
        "PRIMER", "SEGUNDO", "TERCER", "CUARTO", "QUINTO", "SEXTO",
        "SÉPTIMO", "OCTAVO", "NOVENO", "DÉCIMO", "UNDÉCIMO", "DUODÉCIMO",
    ];

    /// <summary>Nombre del mes en minúsculas (1 = enero … 12 = diciembre).</summary>
    public static string MesEnLetras(int mes) =>
        mes is >= 1 and <= 12 ? Meses[mes - 1] : string.Empty;

    /// <summary>
    /// Ordinal del cuatrimestre en mayúsculas ("PRIMER", "SEGUNDO", …). Sucesor de
    /// <c>UpperCase(LetrasCuat(n))</c>. Para valores fuera de rango devuelve el número.
    /// </summary>
    public static string CuatrimestreEnLetras(int cuatrimestre) =>
        cuatrimestre is >= 1 and <= 12
            ? Ordinales[cuatrimestre - 1]
            : cuatrimestre.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
