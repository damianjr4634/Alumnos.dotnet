namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Fila de la grilla de mesas de examen (sucesor del SELECT de
/// MesasExamen.FormActivate: MESAS + sigla de MATERIAS + descripción de MESA_TIPO).
/// </summary>
public sealed record MesaListItemDto
{
    public required string CodigoCarrera { get; init; }

    public int NumeroMesa { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public int? Llamado { get; init; }

    public DateOnly? FechaExamen { get; init; }

    public int? Hora { get; init; }

    public string? Titular { get; init; }

    public string? Vocal1 { get; init; }

    public string? Vocal2 { get; init; }

    public int? Aula { get; init; }

    public short? Cuatrimestre { get; init; }

    public int? Comision1 { get; init; }

    public int? Comision2 { get; init; }

    public int? Comision3 { get; init; }

    public string? CodigoTipo { get; init; }

    public string? DescripcionTipo { get; init; }
}
