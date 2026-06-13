namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Inscripción de un alumno en una materia (sucesor del INSERT de
/// InscripcionDeMaterias.GrabaMateriaClick). El CUTUCO se arma en el handler
/// como 900 + turno×10 + comisión (el trigger CURSADA_BI0 resuelve el 9 con el
/// cuatrimestre del plan); APELLIDO/MATRIZ/INSTITUT/CARAC se completan desde
/// el alumno y su carrera, como hacía el formulario legacy.
/// </summary>
public sealed record InscribirEnMateriaCommand
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    /// <summary>Turno (0-9), segundo dígito del CUTUCO.</summary>
    public required int Turno { get; init; }

    /// <summary>Comisión (0-9), tercer dígito del CUTUCO.</summary>
    public required int NumeroComision { get; init; }

    /// <summary>CUA_ANIO: cuatrimestre (1) + año (2), ej. "124" = 1º/2024.</summary>
    public required string CuatrimestreAnio { get; init; }

    /// <summary>Condición de inscripción: CURSANDO, P/EQUIVALEN o RECURSANDO (combo legacy).</summary>
    public required string Condicion { get; init; }
}
