namespace Esba.Domain.Certificados;

/// <summary>
/// Fila de materia tal como la necesita el reporte tabular "Constancia de Materias
/// Aprobadas" (CMA). Es la proyección de dominio de las filas de
/// <c>XXX_CONSTANCIA_TERCIARIA</c> (la capa Application mapea el DTO del wrapper a
/// este tipo). Más rica que <see cref="MateriaConstancia"/> porque la CMA imprime
/// nota, fecha, instituto y los datos de aprobación por equivalencia/eximición.
/// </summary>
public sealed record MateriaAnaliticoConstancia
{
    /// <summary>Cuatrimestre del plan al que pertenece la materia (para agrupar).</summary>
    public required int Cuatrimestre { get; init; }

    /// <summary>Nombre de la materia que se imprime en la columna "Materia" (DESCRIPCI).</summary>
    public required string Descripcion { get; init; }

    /// <summary>true si es materia anual (ANUAL='*'): ocupa la fila con un texto único.</summary>
    public bool EsAnual { get; init; }

    /// <summary>Condición del alumno en la materia ('* ADEUDA *', 'REGULAR', 'EXIMIDO', 'EQUIVALENCIA', …).</summary>
    public string? Condicion { get; init; }

    /// <summary>Nota del final (0/null se imprime como "------").</summary>
    public decimal? Nota { get; init; }

    /// <summary>Fecha del final (null se imprime como "--------------").</summary>
    public DateOnly? Fecha { get; init; }

    /// <summary>Instituto donde rindió (INSTITUTO).</summary>
    public string? Instituto { get; init; }

    /// <summary>Característica del instituto (CARACT), se concatena al instituto.</summary>
    public string? Caracteristica { get; init; }

    /// <summary>N° de acta interna si la materia se aprobó por equivalencia (ACTINT).</summary>
    public string? ActividadInterna { get; init; }

    /// <summary>N° de acta D.G.E.G.P. si la materia se aprobó por equivalencia (ACTDEGP).</summary>
    public string? ActividadDgegp { get; init; }

    /// <summary>Descripción de la eximición cuando la condición es 'EXIMIDO' (EXIMDESC).</summary>
    public string? EximidoDescripcion { get; init; }
}
