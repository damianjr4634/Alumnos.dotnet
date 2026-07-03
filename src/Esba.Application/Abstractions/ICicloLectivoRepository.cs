using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

/// <summary>Escrituras de los ciclos lectivos (TBL_CUAT/TBL_TRIM).</summary>
public interface ICicloLectivoRepository
{
    /// <summary>Busca el año en TBL_CUAT, trackeado para edición/baja.</summary>
    Task<CicloCuatrimestral?> ObtenerCuatrimestralAsync(int anio, CancellationToken ct);

    /// <summary>Busca el año en TBL_TRIM, trackeado para edición/baja.</summary>
    Task<CicloTrimestral?> ObtenerTrimestralAsync(int anio, CancellationToken ct);

    void Agregar(CicloCuatrimestral ciclo);

    void Agregar(CicloTrimestral ciclo);

    void Eliminar(CicloCuatrimestral ciclo);

    void Eliminar(CicloTrimestral ciclo);
}
