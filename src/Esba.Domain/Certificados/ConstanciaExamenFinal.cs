namespace Esba.Domain.Certificados;

/// <summary>
/// Reglas de la Constancia de Examen Final (CE), sucesor de
/// <c>Impresion_Constancia_Examen</c> de constanciaalumnos2.pas.
/// </summary>
public static class ConstanciaExamenFinal
{
    // Condiciones para las que el legacy NO emite la constancia: la materia todavía
    // no está rendida/aprobada (línea de guarda de Impresion_Constancia_Examen).
    private static readonly HashSet<string> CondicionesNoElegibles = new(StringComparer.OrdinalIgnoreCase)
    {
        "* ADEUDA *", "CURSANDO", "RECURSANDO", "EQUIVALENCIA", "PREVIA",
    };

    /// <summary>
    /// true si la condición de la materia habilita emitir la constancia de examen final.
    /// </summary>
    public static bool EsCondicionElegible(string? condicion) =>
        !CondicionesNoElegibles.Contains((condicion ?? string.Empty).Trim());
}
