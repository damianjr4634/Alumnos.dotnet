namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Fila de regularización de una materia de <b>secundario</b> (carreras 333/650): régimen
/// trimestral (3 trimestres) con exámenes de diciembre y marzo. Sucesora de la carga a
/// "$$$CURSADA" de RegularizacionDeMaterias_nuevo.pas (rama 333/650), sin staging.
/// </summary>
public sealed record Regularizacion333Dto
{
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public short Cutuco { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? Condicion { get; init; }

    /// <summary>1° trimestre (CURSADA.TP_EVA).</summary>
    public decimal? TpEva { get; init; }

    /// <summary>2° trimestre (CURSADA.TP_EVA2) — nota que decide la condición.</summary>
    public decimal? TpEva2 { get; init; }

    /// <summary>3° trimestre (CURSADA.TP_EVA3).</summary>
    public decimal? TpEva3 { get; init; }

    public DateTime? FecEva1 { get; init; }

    public DateTime? FecEva2 { get; init; }

    public DateTime? FecEva3 { get; init; }

    /// <summary>Examen de diciembre (CURSADA.NOTADIC).</summary>
    public decimal? NotaDic { get; init; }

    public DateTime? FechDic { get; init; }

    /// <summary>Examen de marzo (CURSADA.NOTAMAR).</summary>
    public decimal? NotaMar { get; init; }

    public DateTime? FechMar { get; init; }

    public short? TotalHoras { get; init; }

    public short? Inasistencias { get; init; }

    public short? Justificadas { get; init; }

    /// <summary>Fecha de regularización (CURSADA.FECHA1).</summary>
    public DateTime? Fecha { get; init; }
}
