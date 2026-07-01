namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Fila de regularización de una materia de <b>bachillerato</b>: una cursada con las
/// notas del cursado (2 bimestres + recuperatorio + nota "a regular") y las faltas
/// editables. Sucesora de la carga a "$$$CURSADA" de RegularizacionDeMaterias_nuevo.pas
/// (rama BAC), sin staging: se lee de CURSADA y el estado de edición vive en el componente.
/// </summary>
public sealed record RegularizacionBachilleratoDto
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

    /// <summary>Nota del examen "a regular" (CURSADA.REGULAR).</summary>
    public decimal? NotaRegular { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    /// <summary>Nota definitiva ya cargada (CURSADA.FINAL1), si la hubiera.</summary>
    public decimal? NotaDefinitiva { get; init; }

    /// <summary>Fecha de regularización ya cargada (CURSADA.FECHA1), si la hubiera.</summary>
    public DateTime? Fecha { get; init; }

    /// <summary>La cursada figura en la tabla RECURSA (rescate a RECURSANDO de _BAC).</summary>
    public bool EnRecursa { get; init; }
}
