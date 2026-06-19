namespace Esba.Domain.Certificados;

/// <summary>
/// Fila ya formateada para el reporte tabular "Constancia de Materias Aprobadas":
/// el texto de cada celda resuelto según la condición de la materia. El servicio de
/// reporte solo la dibuja (migration_improvements.md §2.1).
/// </summary>
/// <remarks>
/// Cuando <see cref="OcupaFilaCompleta"/> es true (materia anual o aprobada por
/// equivalencia/eximición), <see cref="Condicion"/> lleva el texto único que abarca
/// las columnas de la derecha y <see cref="Nota"/>/<see cref="Fecha"/>/<see cref="Instituto"/>
/// quedan vacíos.
/// </remarks>
public sealed record FilaAnaliticoConstancia
{
    /// <summary>Cuatrimestre del plan (encabezado de grupo del reporte).</summary>
    public required int Cuatrimestre { get; init; }

    /// <summary>Texto de la columna "Materia" (nombre de la materia).</summary>
    public required string Materia { get; init; }

    /// <summary>Texto de la columna "Condición" (o el texto único si <see cref="OcupaFilaCompleta"/>).</summary>
    public required string Condicion { get; init; }

    /// <summary>Texto de la columna "Nota".</summary>
    public string Nota { get; init; } = string.Empty;

    /// <summary>Texto de la columna "Fecha".</summary>
    public string Fecha { get; init; } = string.Empty;

    /// <summary>Texto de la columna "Instituto / Característica".</summary>
    public string Instituto { get; init; } = string.Empty;

    /// <summary>true si <see cref="Condicion"/> abarca las columnas de la derecha (anual/equivalencia/eximido).</summary>
    public bool OcupaFilaCompleta { get; init; }
}
