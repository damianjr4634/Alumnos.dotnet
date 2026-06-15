namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Fila de XXX_MATERIAS_FINALES: una materia que el alumno podría rendir en final,
/// con el resultado de la validación de correlatividad y la mesa asignada.
/// </summary>
public sealed record MateriaFinalDto
{
    public required string CodigoMateria { get; init; }

    public string? Materia { get; init; }

    /// <summary>FERRCOD = 0 ⇒ puede rendir; distinto ⇒ no (ver <see cref="Mensaje"/>).</summary>
    public bool PuedeRendir { get; init; }

    public string? Mensaje { get; init; }

    public int? Cutuco { get; init; }

    public string? Condicion { get; init; }

    /// <summary>FMESA: número de mesa donde rinde.</summary>
    public int? Mesa { get; init; }
}
