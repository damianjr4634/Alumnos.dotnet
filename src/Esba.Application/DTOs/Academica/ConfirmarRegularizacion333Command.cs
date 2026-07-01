namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Notas del cursado editadas para una materia de secundario (333/650) de un alumno:
/// 3 trimestres + exámenes de diciembre/marzo. Las notas vacías son null y 99 es "ausente".
/// </summary>
public sealed record NotaCursado333Input
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? CondicionActual { get; init; }

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

    /// <summary>Fecha de regularización (CURSADA.FECHA1).</summary>
    public DateTime? Fecha { get; init; }

    /// <summary>
    /// Override manual "pasar a Previa" (botón del formulario legacy): fuerza CONDICION=PREVIA
    /// y marca el examen de marzo pendiente (NOTAMAR=99), sin evaluar el ladder.
    /// </summary>
    public bool ForzarPrevia { get; init; }
}

/// <summary>
/// Confirma la regularización de una o varias materias de secundario (333/650): calcula la
/// condición resultante (dominio) y vuelca a CURSADA/ANALITIC. Sucesor de la grabación por
/// materia + XXX_REGULARIZACION_MAT_333 + el commit XXX_REGULARIZACION (rama 333/650).
/// </summary>
public sealed record ConfirmarRegularizacion333Command
{
    public required string CodigoCarrera { get; init; }

    public required int CodigoUsuario { get; init; }

    public required IReadOnlyList<NotaCursado333Input> Filas { get; init; }
}
