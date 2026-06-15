namespace Esba.Application.DTOs.Asistencias;

/// <summary>
/// Fila de la planilla de reincorporaciones/libres (XXX_FALTAS_IMPRESI): alumno
/// que alcanzó un umbral de inasistencias, con el desglose por tipo.
/// </summary>
public sealed record PlanillaReincorporacionDto
{
    public required string CodigoAlumno { get; init; }

    public string? Nombre { get; init; }

    public int Cutuco { get; init; }

    public decimal Justificadas { get; init; }

    public decimal Injustificadas { get; init; }

    public decimal Tardanzas { get; init; }

    public decimal EducacionFisica { get; init; }

    public decimal Total { get; init; }

    /// <summary>Estado de reincorporación: "por primera vez", "por segunda vez" o "libre".</summary>
    public string? Estado { get; init; }

    /// <summary>Fecha en que cruzó el umbral.</summary>
    public DateOnly? Fecha { get; init; }

    public int ActasDisciplina { get; init; }
}
