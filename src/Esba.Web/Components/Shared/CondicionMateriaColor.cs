using MudBlazor;

namespace Esba.Web.Components.Shared;

/// <summary>
/// Color semántico del badge de la condición de una materia, compartido por las
/// pantallas que la muestran (cursada del alumno y analítico de la constancia).
/// Deriva del texto de la condición con colores del tema (§4.5), en vez del RGB del
/// VCL que calcula el SP. Cubre las condiciones de las tres ramas de
/// <c>XXX_CONSTANCIA_TERCIARIA</c> (TER, ADM y BAC/BAD).
/// </summary>
public static class CondicionMateriaColor
{
    public static Color Para(string? condicion) => (condicion ?? string.Empty).Trim().ToUpperInvariant() switch
    {
        "CURSANDO" => Color.Success,
        "REGULAR" => Color.Info,
        "RECURSANDO" or "RECURSA" or "PREVIA" or "A/REGULAR" => Color.Warning,
        "LIBRE" or "LIBRES" or "* ADEUDA *" or "PREVIO" or "DICIEMBRE" or "MARZO" or "P/EQUIVALEN" => Color.Error,
        "EQUIVALENCIA" or "EXIMIDO" => Color.Secondary,
        "**INSCRIPTO**" => Color.Tertiary,
        _ => Color.Default,
    };
}
