namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Notas del cursado editadas para una materia de un alumno (entrada de la
/// regularización). Los flags de la materia y la condición actual llegan desde la
/// carga (RegularizacionCursadaDto); las notas vacías son null y 99 es "ausente".
/// </summary>
public sealed record NotaCursadoInput
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? CondicionActual { get; init; }

    public decimal? TpEva { get; init; }

    public decimal? TpEva2 { get; init; }

    public decimal? Recuperatorio { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    public bool MateriaPromociona { get; init; }

    public bool MateriaApruebaSinFinal { get; init; }

    /// <summary>Override manual "pasar a Libre" (BtnLibre del formulario legacy por-alumno).</summary>
    public bool ForzarLibre { get; init; }
}

/// <summary>
/// Confirma la regularización de una o varias materias terciarias: calcula la condición
/// resultante (dominio) y vuelca a CURSADA/ANALITIC. Sucesor de ValidoGrabaciondeMateria
/// + el commit XXX_REGULARIZACION de RegularizacionDeMaterias_nuevo.pas / XComision.
/// </summary>
public sealed record ConfirmarRegularizacionCommand
{
    public required string CodigoCarrera { get; init; }

    public required int CodigoUsuario { get; init; }

    public required IReadOnlyList<NotaCursadoInput> Filas { get; init; }
}
