namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Fila de la grilla de cursada de un alumno (sucesor de la grilla de
/// InscripcionDeMaterias.pas): la materia, la comisión y el estado del cursado.
/// </summary>
public sealed record CursadaListItemDto
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public short? Cutuco { get; init; }

    public string? CuatrimestreAnio { get; init; }

    public required string Condicion { get; init; }

    public decimal? Evaluacion1 { get; init; }

    public decimal? Evaluacion2 { get; init; }

    public decimal? NotaRegular { get; init; }

    public decimal? NotaFinal1 { get; init; }

    public DateOnly? FechaFinal1 { get; init; }

    public short? Inasistencias { get; init; }
}
