namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Fila de la grilla de comisiones armadas (sucesor del SELECT de
/// cargacomisiones.pas: COMARM + sigla de MATERIAS + nombre de DOCENTES).
/// </summary>
public sealed record ComisionListItemDto
{
    public required string CodigoCarrera { get; init; }

    public short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? CodigoProfesor { get; init; }

    /// <summary>Nombre del docente (JOIN a DOCENTES, tabla aún no migrada — la query Dapper lo trae igual).</summary>
    public string? Docente { get; init; }

    public string? Dia1 { get; init; }

    public string? Bloque1 { get; init; }

    public string? Dia2 { get; init; }

    public string? Bloque2 { get; init; }

    public string? Dia3 { get; init; }

    public string? Bloque3 { get; init; }

    public string? TitularSuplente { get; init; }
}
