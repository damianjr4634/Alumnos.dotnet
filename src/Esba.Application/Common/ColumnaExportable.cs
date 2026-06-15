namespace Esba.Application.Common;

/// <summary>
/// Descriptor de una columna a exportar (Excel/PDF). Define el título y cómo
/// extraer el valor de cada fila, sin acoplar Application a ClosedXML/QuestPDF
/// ni a la grilla de UI. Sucesor del armado de columnas que <c>modulovariable</c>
/// hacía en runtime sobre el dataset (Modulo Variable/modulovariable.pas).
/// </summary>
public sealed class ColumnaExportable<T>
{
    public required string Titulo { get; init; }

    /// <summary>Extrae el valor crudo de la fila (string, número, fecha, bool…).</summary>
    public required Func<T, object?> Valor { get; init; }

    /// <summary>
    /// Formato .NET opcional aplicado al valor (ej. "dd/MM/yyyy", "N2"). Si es
    /// null se usa la representación por defecto del tipo.
    /// </summary>
    public string? Formato { get; init; }

    /// <summary>Alinear a la derecha (números). Por defecto izquierda.</summary>
    public bool AlinearDerecha { get; init; }

    /// <summary>Texto ya formateado de una fila, listo para volcar en Excel/PDF.</summary>
    public string Formatear(T fila)
    {
        var valor = Valor(fila);
        if (valor is null)
        {
            return string.Empty;
        }

        if (Formato is not null && valor is IFormattable formateable)
        {
            return formateable.ToString(Formato, System.Globalization.CultureInfo.CurrentCulture);
        }

        return valor switch
        {
            bool b => b ? "Sí" : "No",
            _ => valor.ToString() ?? string.Empty,
        };
    }
}
