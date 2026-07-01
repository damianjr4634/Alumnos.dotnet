namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Fila de regularización: una cursada de un alumno en una materia, con las notas del
/// cursado y las faltas editables. Sucesor de la carga a la tabla staging "$$$CURSADA"
/// de RegularizacionDeMaterias_nuevo.pas (aquí sin staging: se lee de CURSADA y el
/// estado de edición vive en el componente).
/// </summary>
public sealed record RegularizacionCursadaDto
{
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public short Cutuco { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? Condicion { get; init; }

    public decimal? TpEva { get; init; }

    public decimal? TpEva2 { get; init; }

    public decimal? Recuperatorio { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    /// <summary>MATERIAS.PROMOCION = 'S': admite promoción sin final.</summary>
    public bool MateriaPromociona { get; init; }

    /// <summary>MATERIAS.APRSFINAL = 'S': aprueba sin rendir final.</summary>
    public bool MateriaApruebaSinFinal { get; init; }
}
