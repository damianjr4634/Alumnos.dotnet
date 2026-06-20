namespace Esba.Domain.Certificados;

/// <summary>
/// Composición de los textos de la equivalencia bachiller, sucesores de los literales
/// y el reformateo embebidos en <c>ImprimirClick</c> de lst_impresion_equivalencia_bac.pas.
/// Lógica pura (sin I/O): se prueba sin base ni QuestPDF.
/// </summary>
public static class EquivalenciaBachillerFormatter
{
    private const string VistaConstanciaEnTramite =
        "y teniendo a la vista la (*) constancia de título en trámite otorgado por ";

    private const string VistaCertificadoAnalitico =
        "y teniendo a la vista el Certificado Analítico del nivel medio otorgado por ";

    public const string NotaAdReferendum =
        "(*) NOTA: Condicional AD-REFERENDUM del Certificado Analítico";

    /// <summary>'BAC' (bachiller) y 'BAD' (bachiller a distancia) habilitan la impresión; 'TER' no.</summary>
    public static bool EsTipoBachiller(string? tipoCarrera) =>
        (tipoCarrera ?? string.Empty).Trim().ToUpperInvariant() is "BAC" or "BAD";

    /// <summary>true si el alumno presentó el título secundario en trámite (A_C = 'C'), no el analítico.</summary>
    public static bool EsTituloEnTramite(string? documentoAC) =>
        string.Equals((documentoAC ?? string.Empty).Trim(), "C", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// ACTINT → "número/AA": separa los dos últimos dígitos como año.
    /// Sucesor de <c>COPY(actint,1,len-2)+'/'+COPY(actint,len-1,2)</c>; conserva los
    /// ceros a la izquierda que dejó el trigger (paridad con el legacy).
    /// </summary>
    public static string FormatearResolucionInterna(string? actividadInterna)
    {
        var texto = (actividadInterna ?? string.Empty).Trim();
        return texto.Length >= 2 ? $"{texto[..^2]}/{texto[^2..]}" : texto;
    }

    /// <summary>
    /// Frase "y teniendo a la vista …" según el documento secundario presentado, seguida
    /// de la institución de origen y la descripción del plan.
    /// </summary>
    public static string TextoVista(string? documentoAC, string? instituto, string? colegio, string? plan)
    {
        var origen = string.IsNullOrWhiteSpace(instituto) ? colegio : instituto;
        var sufijo = $"{(origen ?? string.Empty).Trim()} {(plan ?? string.Empty).Trim()}".Trim();
        var encabezado = EsTituloEnTramite(documentoAC) ? VistaConstanciaEnTramite : VistaCertificadoAnalitico;
        return encabezado + sufijo;
    }
}
