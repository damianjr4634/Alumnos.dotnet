using Esba.Domain.Enums;

namespace Esba.Application.DTOs.AreaDocente;

/// <summary>
/// Filtro + paginación del listado de precargas de docentes que ve secretaría
/// (sucesor del "a procesar" implícito del legacy: no existía, los docentes avisaban a mano).
/// </summary>
public sealed record PrecargasDocenteFiltro
{
    /// <summary>Carreras habilitadas al usuario (BARRA_SEGU); null = todas (supervisor).</summary>
    public IReadOnlyCollection<string>? CarrerasPermitidas { get; init; }

    public string? CodigoCarrera { get; init; }

    /// <summary>null = todos los estados.</summary>
    public EstadoCargaDocente? Estado { get; init; }

    /// <summary>Texto libre sobre docente o materia.</summary>
    public string? Texto { get; init; }

    public string? OrdenarPor { get; init; }

    public bool Descendente { get; init; }

    public int Skip { get; init; }

    public int Take { get; init; } = 25;
}

/// <summary>Fila del listado de precargas por comisión.</summary>
public sealed record PrecargaComisionListItemDto
{
    public int CargaId { get; init; }

    public required string CodigoCarrera { get; init; }

    public string? NombreCarrera { get; init; }

    public short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? CodigoDocente { get; init; }

    public string? NombreDocente { get; init; }

    /// <summary>ESTADO crudo ('BOR'/'FIN'/'EFE'); <see cref="Estado"/> lo traduce.</summary>
    public required string EstadoCodigo { get; init; }

    public DateTime? FechaModificacion { get; init; }

    public DateTime? FechaFinalizacion { get; init; }

    public DateTime? FechaEfectivizacion { get; init; }

    /// <summary>Alumnos con algo precargado.</summary>
    public int CantidadAlumnos { get; init; }

    public EstadoCargaDocente Estado => EstadoCargaDocenteCodigo.DesdeCodigo(EstadoCodigo);
}

/// <summary>Fila del listado de precargas por mesa.</summary>
public sealed record PrecargaMesaListItemDto
{
    public int CargaId { get; init; }

    public required string CodigoCarrera { get; init; }

    public string? NombreCarrera { get; init; }

    public int NumeroMesa { get; init; }

    public string? CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public DateOnly? FechaExamen { get; init; }

    public string? CodigoDocente { get; init; }

    public string? NombreDocente { get; init; }

    public required string EstadoCodigo { get; init; }

    public DateTime? FechaModificacion { get; init; }

    public DateTime? FechaFinalizacion { get; init; }

    public DateTime? FechaEfectivizacion { get; init; }

    public int CantidadAlumnos { get; init; }

    public EstadoCargaDocente Estado => EstadoCargaDocenteCodigo.DesdeCodigo(EstadoCodigo);
}
