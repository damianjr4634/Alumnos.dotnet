namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Vista del analítico del alumno para la pantalla de constancia: las materias del
/// plan con su condición (<see cref="ConstanciaMateriaDto"/>) más el promedio general.
/// Sucesor de la grilla de materias + campo "Promedio General" de constanciaalumnos2.pas.
/// </summary>
public sealed record AnaliticoAlumnoModel
{
    public IReadOnlyList<ConstanciaMateriaDto> Materias { get; init; } = [];

    public decimal PromedioGeneral { get; init; }
}
