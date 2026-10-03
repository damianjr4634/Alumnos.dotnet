namespace Esba.Application.DTOs.AreaDocente;

/// <summary>Nota de final precargada para un alumno con permiso en la mesa (una fila de la grilla).</summary>
public sealed record FilaCargaMesaInput
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public int? PermisoIndice { get; init; }

    /// <summary>Nota en [1,10]; null si no rindió todavía o está ausente.</summary>
    public decimal? Nota { get; init; }

    /// <summary>No se presentó. Excluyente con <see cref="Nota"/>.</summary>
    public bool Ausente { get; init; }

    public string? Observaciones { get; init; }
}

/// <summary>
/// Guarda el borrador de una mesa: crea la cabecera en el primer guardado y hace upsert de
/// los detalles por alumno+materia. Quién puede lo decide el handler a partir del actor.
/// </summary>
public sealed record GuardarCargaMesaCommand
{
    public required ClaveMesa Mesa { get; init; }

    public required ActorCargaDocente Actor { get; init; }

    public string? Observaciones { get; init; }

    public IReadOnlyList<FilaCargaMesaInput> Filas { get; init; } = [];
}

/// <summary>Cambio de estado de la carga de una mesa (finalizar / reabrir).</summary>
public sealed record CambiarEstadoCargaMesaCommand
{
    public required ClaveMesa Mesa { get; init; }

    public required ActorCargaDocente Actor { get; init; }
}
