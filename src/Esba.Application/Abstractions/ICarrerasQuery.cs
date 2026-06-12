using Esba.Application.DTOs.Carreras;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas de carreras para el menú por carrera (sucesor de Carreras.pas /
/// BarraCarrerasItems del shell legacy).
/// </summary>
public interface ICarrerasQuery
{
    /// <summary>
    /// Carreras activas agrupables para el menú. <paramref name="codigosPermitidos"/>
    /// null = supervisor (ve todas); si no, solo las habilitadas en BARRA_SEGU.
    /// </summary>
    Task<IReadOnlyList<CarreraListItemDto>> ListarParaMenuAsync(IReadOnlyCollection<string>? codigosPermitidos, CancellationToken ct);
}
