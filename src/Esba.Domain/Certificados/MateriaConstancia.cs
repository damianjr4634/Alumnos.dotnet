namespace Esba.Domain.Certificados;

/// <summary>
/// Fila de materia tal como la necesita el cálculo de "materias que adeuda" de la
/// constancia. Es la proyección de dominio de las filas de <c>XXX_CONSTANCIA_TERCIARIA</c>
/// (la capa Application mapea el DTO del wrapper a este tipo). El orden de la
/// secuencia debe respetar el del SP (ORDER BY cuatrimestre).
/// </summary>
public sealed record MateriaConstancia
{
    /// <summary>Cuatrimestre del plan al que pertenece la materia.</summary>
    public required int Cuatrimestre { get; init; }

    /// <summary>Sigla/abreviatura de la materia (lo que se lista al alumno).</summary>
    public required string Sigla { get; init; }

    /// <summary>Condición del alumno en la materia ('* ADEUDA *', 'CURSANDO', 'REGULAR', …).</summary>
    public string? Condicion { get; init; }

    /// <summary>Nota del final (0/null cuenta como pendiente).</summary>
    public decimal? Nota { get; init; }
}
