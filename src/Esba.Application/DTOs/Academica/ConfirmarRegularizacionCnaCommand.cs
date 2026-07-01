namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Nota final editada para una materia de CNA de un alumno (entrada de la regularización).
/// La condición se deriva de la nota; la fecha es obligatoria.
/// </summary>
public sealed record NotaCursadoCnaInput
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? CondicionActual { get; init; }

    /// <summary>Nota final (CURSADA.FINAL1 y NOTA_MAT del analítico si queda REGULAR).</summary>
    public decimal? NotaFinal { get; init; }

    /// <summary>Fecha del examen final (CURSADA.FECHA1 y FEC_FINAL del analítico). Obligatoria.</summary>
    public DateTime? Fecha { get; init; }
}

/// <summary>
/// Confirma la regularización de una o varias materias de CNA: deriva la condición de la
/// nota final (dominio) y vuelca a CURSADA/ANALITIC. Sucesor de GrabaMateriaCNAClick + el
/// commit XXX_REGULARIZACION (rama BAC, ya que CNA es CARRERA.TIPO='BAC').
/// </summary>
public sealed record ConfirmarRegularizacionCnaCommand
{
    public required string CodigoCarrera { get; init; }

    public required int CodigoUsuario { get; init; }

    public required IReadOnlyList<NotaCursadoCnaInput> Filas { get; init; }
}
