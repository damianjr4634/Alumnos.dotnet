namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Filtros del parte diario de mesas (sucesor del panel de parámetros de
/// dxBarButton21Click en FrmEsba.pas: carrera opcional y rango de fechas).
/// </summary>
public sealed record GenerarParteDiarioMesasCommand
{
    /// <summary>Carrera. Opcional: vacío incluye todas.</summary>
    public string? CodigoCarrera { get; init; }

    public required DateOnly FechaDesde { get; init; }

    public required DateOnly FechaHasta { get; init; }
}
