namespace Esba.Application.DTOs.AreaDocente;

/// <summary>Una mesa de examen donde el docente es titular (MESAS + materia + tipo + resumen de PERMEXA y de su precarga).</summary>
public sealed record MesaDocenteDto
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

    /// <summary>Alumnos con permiso de examen (PERMEXA) para la mesa.</summary>
    public int CantidadInscriptos { get; init; }

    /// <summary>DOC_CARGA_MESA.ESTADO ('BOR'/'FIN'/'EFE'); null = sin precarga.</summary>
    public string? EstadoCarga { get; init; }
}
