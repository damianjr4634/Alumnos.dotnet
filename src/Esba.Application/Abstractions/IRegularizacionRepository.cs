namespace Esba.Application.Abstractions;

/// <summary>
/// Una cursada terciaria ya resuelta (condición calculada por el dominio) lista para
/// volcarse. Sucesor de una fila de "$$$CURSADA" que el SP de commit XXX_REGULARIZACION
/// procesaba.
/// </summary>
public sealed record FilaRegularizacionResuelta
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public decimal? TpEva { get; init; }

    public decimal? TpEva2 { get; init; }

    public decimal? Recuperatorio { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    public required string NuevaCondicion { get; init; }

    /// <summary>Nota que va al analítico si la materia se aprueba directo (PROMOCIONA/FINAL); null si no.</summary>
    public decimal? NotaAnalitico { get; init; }
}

/// <summary>
/// Una cursada de bachillerato ya resuelta (condición calculada por el dominio) lista
/// para volcarse. Sucesor de una fila de "$$$CURSADA" que la rama BAC del SP de commit
/// XXX_REGULARIZACION procesaba.
/// </summary>
public sealed record FilaRegularizacionBachilleratoResuelta
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public decimal? TpEva { get; init; }

    public decimal? TpEva2 { get; init; }

    public decimal? Recuperatorio { get; init; }

    public decimal? NotaRegular { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    /// <summary>Fecha de regularización (CURSADA.FECHA1 y FEC_FINAL del analítico si aprueba).</summary>
    public DateTime? Fecha { get; init; }

    public required string NuevaCondicion { get; init; }

    /// <summary>Nota definitiva (CURSADA.FINAL1 y NOTA_MAT del analítico si la materia queda REGULAR).</summary>
    public decimal? NotaFinal { get; init; }
}

/// <summary>
/// Volcado de la regularización de materias (porta las ramas TER y BAC del SP
/// XXX_REGULARIZACION a C#, sin el staging "$$$CURSADA"). Todas las filas se procesan
/// en una sola transacción.
/// </summary>
public interface IRegularizacionRepository
{
    /// <summary>Vuelca la regularización terciaria. Devuelve la cantidad de filas procesadas.</summary>
    Task<int> ConfirmarTerciariaAsync(
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacionResuelta> filas,
        CancellationToken ct);

    /// <summary>
    /// Vuelca la regularización de bachillerato (rama BAC del commit): UPDATE CURSADA y,
    /// si la materia queda REGULAR, la mueve a CURSADA_HST e inserta en ANALITIC (nota
    /// FINAL1, fecha FECHA1). Devuelve la cantidad de filas procesadas.
    /// </summary>
    Task<int> ConfirmarBachilleratoAsync(
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacionBachilleratoResuelta> filas,
        CancellationToken ct);
}
