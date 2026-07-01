namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Notas del cursado editadas para una materia de bachillerato de un alumno (entrada de
/// la regularización). Los flags/condición llegan desde la carga (RegularizacionBachilleratoDto);
/// las notas vacías son null y 99 es "ausente".
/// </summary>
public sealed record NotaCursadoBachilleratoInput
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? CondicionActual { get; init; }

    public decimal? TpEva { get; init; }

    public decimal? TpEva2 { get; init; }

    public decimal? Recuperatorio { get; init; }

    public decimal? NotaRegular { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    /// <summary>Fecha de regularización (CURSADA.FECHA1 / FEC_FINAL del analítico si aprueba).</summary>
    public DateTime? Fecha { get; init; }

    /// <summary>La cursada figura en RECURSA (rescate a RECURSANDO de _BAC).</summary>
    public bool EnRecursa { get; init; }

    /// <summary>
    /// Decisión del operador cuando las faltas dejan al alumno en CONSEJO:
    /// null/"" pide la decisión; "Consejo"/"Regular"/"Libre" la resuelven (PASO de _POSTVAL).
    /// </summary>
    public string? Paso { get; init; }

    /// <summary>
    /// Override manual "pasar a Libre" (botón del formulario legacy): fuerza CONDICION=LIBRE
    /// y las notas a 99, sin evaluar el ladder. Solo aplica sobre CURSANDO/RECURSANDO.
    /// </summary>
    public bool ForzarLibre { get; init; }
}

/// <summary>
/// Confirma la regularización de una o varias materias de bachillerato: calcula la
/// condición resultante (dominio) y vuelca a CURSADA/ANALITIC. Sucesor de
/// GrabaMateriaBac + _BAC/_POSTVAL + el commit XXX_REGULARIZACION (rama BAC).
/// </summary>
public sealed record ConfirmarRegularizacionBachilleratoCommand
{
    public required string CodigoCarrera { get; init; }

    public required int CodigoUsuario { get; init; }

    public required IReadOnlyList<NotaCursadoBachilleratoInput> Filas { get; init; }
}
