namespace Esba.Application.DTOs.Examenes;

/// <summary>Permiso de examen de un alumno (PERMEXA + sigla/descripción de la materia).</summary>
public sealed record PermisoExamenDto
{
    public int? NumeroPermiso { get; init; }

    public int Mesa { get; init; }

    public int? Llamado { get; init; }

    public int? Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public string? Materia { get; init; }

    public DateOnly? FechaExamen { get; init; }

    public DateOnly? FechaEmision { get; init; }
}
