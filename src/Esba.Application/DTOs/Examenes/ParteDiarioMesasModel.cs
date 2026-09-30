namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Modelo del parte diario de mesas listo para maquetar (sucesor de
/// Imp_Mesas_ParteDiario): una hoja por fecha de examen, con las mesas agrupadas por carrera.
/// </summary>
public sealed record ParteDiarioMesasModel
{
    /// <summary>Año en curso ("CURSO LECTIVO").</summary>
    public required int CicloLectivo { get; init; }

    public required IReadOnlyList<ParteDiarioDia> Dias { get; init; }
}

public sealed record ParteDiarioDia
{
    public DateOnly? Fecha { get; init; }

    public required IReadOnlyList<ParteDiarioCarrera> Carreras { get; init; }
}

public sealed record ParteDiarioCarrera
{
    public required string CodigoCarrera { get; init; }

    public string? NombreCarrera { get; init; }

    public required IReadOnlyList<ParteDiarioMesaDto> Mesas { get; init; }
}
