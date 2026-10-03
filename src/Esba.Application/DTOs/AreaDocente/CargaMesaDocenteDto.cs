using Esba.Domain.Enums;

namespace Esba.Application.DTOs.AreaDocente;

/// <summary>
/// Pantalla de precarga de una mesa: datos de la mesa (MESAS + carrera + materia + tipo +
/// tribunal), la cabecera de la carga si existe y una fila por alumno con permiso de
/// examen (PERMEXA) con su condición actual y lo precargado.
/// </summary>
public sealed record CargaMesaDocenteDto
{
    public required string CodigoCarrera { get; init; }

    public string? NombreCarrera { get; init; }

    public int NumeroMesa { get; init; }

    public string? CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public string? NombreMateria { get; init; }

    public int? Llamado { get; init; }

    public DateOnly? FechaExamen { get; init; }

    /// <summary>HORA NUMERIC(4,0) legacy, p. ej. 1830 = 18:30.</summary>
    public int? Hora { get; init; }

    public int? Aula { get; init; }

    public string? DescripcionTipo { get; init; }

    /// <summary>MESAS.TITULAR: solo él (o secretaría) carga.</summary>
    public string? CodigoDocenteTitular { get; init; }

    public string? NombreDocenteTitular { get; init; }

    public string? Vocal1 { get; init; }

    public string? Vocal2 { get; init; }

    /// <summary>DOC_CARGA_MESA.ID; null = todavía no hay carga.</summary>
    public int? CargaId { get; init; }

    public EstadoCargaDocente? Estado { get; init; }

    public string? ObservacionesCarga { get; init; }

    public DateTime? FechaModificacion { get; init; }

    public DateTime? FechaFinalizacion { get; init; }

    public DateTime? FechaEfectivizacion { get; init; }

    public IReadOnlyList<AlumnoCargaMesaDto> Alumnos { get; init; } = [];

    public bool TieneCarga => CargaId is not null;
}

/// <summary>Un alumno con permiso en la mesa (PERMEXA ⨝ CURSADA ⨝ ALUMNOS) y, si existe, su fila precargada.</summary>
public sealed record AlumnoCargaMesaDto
{
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    public required string CodigoMateria { get; init; }

    /// <summary>CURSADA.CONDICION actual (REGULAR, LIBRE, PREVIA...).</summary>
    public string? Condicion { get; init; }

    public int PermisoIndice { get; init; }

    public int? Llamado { get; init; }

    /// <summary>DOC_CARGA_MESA_DET.ID; null = el docente todavía no cargó nada para este alumno.</summary>
    public int? DetalleId { get; init; }

    public decimal? Nota { get; init; }

    public bool Ausente { get; init; }

    public string? Observaciones { get; init; }

    public bool TieneDetalle => DetalleId is not null;
}
