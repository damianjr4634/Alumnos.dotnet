namespace Esba.Application.DTOs.Docente;

/// <summary>Una comisión a cargo del docente (COMARM + carrera + materia + resumen de CURSADA y de su precarga).</summary>
public sealed record ComisionDocenteDto
{
    public required string CodigoCarrera { get; init; }

    public string? NombreCarrera { get; init; }

    public short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public string? NombreMateria { get; init; }

    /// <summary>CUA_ANIO ("226" = 2º cuatrimestre 2026).</summary>
    public required string CuatrimestreAnio { get; init; }

    public string? Dia1 { get; init; }

    public string? Bloque1 { get; init; }

    public string? Dia2 { get; init; }

    public string? Bloque2 { get; init; }

    public string? Dia3 { get; init; }

    public string? Bloque3 { get; init; }

    /// <summary>Alumnos CURSANDO/RECURSANDO en la comisión.</summary>
    public int CantidadAlumnos { get; init; }

    /// <summary>DOC_CARGA_COMISION.ESTADO ('BOR'/'FIN'/'EFE'); null = el docente todavía no empezó la precarga.</summary>
    public string? EstadoCarga { get; init; }
}
