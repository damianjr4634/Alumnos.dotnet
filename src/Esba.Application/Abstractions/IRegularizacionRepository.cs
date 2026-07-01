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
/// Volcado de la regularización de materias terciarias (porta la rama TER del SP
/// XXX_REGULARIZACION a C#, sin el staging "$$$CURSADA"). Todas las filas se procesan
/// en una sola transacción.
/// </summary>
public interface IRegularizacionRepository
{
    /// <summary>Devuelve la cantidad de filas procesadas.</summary>
    Task<int> ConfirmarTerciariaAsync(
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacionResuelta> filas,
        CancellationToken ct);
}
