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
/// Una cursada de secundario (333/650) ya resuelta (condición calculada por el dominio)
/// lista para volcarse. Sucesor de una fila de "$$$CURSADA" que la rama 333/650 del SP de
/// commit XXX_REGULARIZACION procesaba.
/// </summary>
public sealed record FilaRegularizacion333Resuelta
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public decimal? TpEva { get; init; }

    public decimal? TpEva2 { get; init; }

    public decimal? TpEva3 { get; init; }

    public DateTime? FecEva1 { get; init; }

    public DateTime? FecEva2 { get; init; }

    public DateTime? FecEva3 { get; init; }

    public decimal? NotaDic { get; init; }

    public DateTime? FechDic { get; init; }

    public decimal? NotaMar { get; init; }

    public DateTime? FechMar { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    public DateTime? Fecha { get; init; }

    public required string NuevaCondicion { get; init; }

    /// <summary>Nota final al analítico (NOTAFIN) si la materia queda REGULAR.</summary>
    public decimal NotaFinal { get; init; }

    /// <summary>Fecha de la nota final (NOTAFIN_FECHA): FEC_EVA2, FECHDIC o FECHMAR según el caso.</summary>
    public DateTime? NotaFinalFecha { get; init; }
}

/// <summary>
/// Una materia de CNA ya resuelta (condición derivada de la nota final) lista para
/// volcarse. El commit usa la rama BAC de XXX_REGULARIZACION.
/// </summary>
public sealed record FilaRegularizacionCnaResuelta
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public required string NuevaCondicion { get; init; }

    /// <summary>Nota final (CURSADA.FINAL1 y NOTA_MAT del analítico si queda REGULAR).</summary>
    public decimal NotaFinal { get; init; }

    /// <summary>Fecha del examen final (CURSADA.FECHA1 y FEC_FINAL del analítico).</summary>
    public DateTime? Fecha { get; init; }
}

/// <summary>
/// Volcado de la regularización de materias (porta las ramas TER, BAC, 333/650 y CNA del SP
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

    /// <summary>
    /// Vuelca la regularización de secundario 333/650 (rama 333/650 del commit): UPDATE
    /// CURSADA y, si la materia queda REGULAR, la mueve a CURSADA_HST e inserta en ANALITIC
    /// (nota NOTAFIN, fecha NOTAFIN_FECHA). Devuelve la cantidad de filas procesadas.
    /// </summary>
    Task<int> Confirmar333Async(
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacion333Resuelta> filas,
        CancellationToken ct);

    /// <summary>
    /// Vuelca la regularización de CNA (rama BAC del commit): UPDATE CURSADA (nota final,
    /// fecha y condición) y, si queda REGULAR, la mueve a CURSADA_HST e inserta en ANALITIC
    /// (nota FINAL1, fecha FECHA1). Devuelve la cantidad de filas procesadas.
    /// </summary>
    Task<int> ConfirmarCnaAsync(
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacionCnaResuelta> filas,
        CancellationToken ct);
}
