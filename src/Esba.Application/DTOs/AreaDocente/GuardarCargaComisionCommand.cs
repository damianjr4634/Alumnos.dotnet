namespace Esba.Application.DTOs.AreaDocente;

/// <summary>
/// Valores precargados para un alumno de la comisión (una fila de la grilla). Qué
/// campos llegan con valor depende de la variante de la carrera (EsquemaCargaCursado);
/// los demás viajan en null.
/// </summary>
public sealed record FilaCargaComisionInput
{
    public required string CodigoAlumno { get; init; }

    public int? CursadaIndice { get; init; }

    public decimal? Evaluacion1 { get; init; }

    public decimal? Recuperatorio { get; init; }

    public decimal? Evaluacion2 { get; init; }

    public decimal? Evaluacion3 { get; init; }

    /// <summary>Nota "a regularizar" (bachillerato).</summary>
    public decimal? NotaRegular { get; init; }

    /// <summary>Nota final (CNA).</summary>
    public decimal? NotaFinal { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    public string? Observaciones { get; init; }
}

/// <summary>
/// Guarda el borrador de una comisión: crea la cabecera en el primer guardado y hace
/// upsert de los detalles por alumno. Quién puede (titular en borrador, o secretaría
/// siempre) lo decide el handler a partir del <see cref="Actor"/>.
/// </summary>
public sealed record GuardarCargaComisionCommand
{
    public required ClaveComision Comision { get; init; }

    public required ActorCargaDocente Actor { get; init; }

    public string? Observaciones { get; init; }

    public IReadOnlyList<FilaCargaComisionInput> Filas { get; init; } = [];
}

/// <summary>Cambio de estado de la carga de una comisión (finalizar / reabrir).</summary>
public sealed record CambiarEstadoCargaComisionCommand
{
    public required ClaveComision Comision { get; init; }

    public required ActorCargaDocente Actor { get; init; }
}
