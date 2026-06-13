namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Modificación de una inscripción (turno/comisión, cuatrimestre y condición).
/// A diferencia del legacy, NO permite cambiar la materia (era un UPDATE de la
/// PK): para eso se elimina la inscripción y se crea otra — desviación
/// documentada en docs/migracion/etapa2-3-inscripcion-materias.md.
/// </summary>
public sealed record ModificarInscripcionCommand
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required int Turno { get; init; }

    public required int NumeroComision { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public required string Condicion { get; init; }
}
