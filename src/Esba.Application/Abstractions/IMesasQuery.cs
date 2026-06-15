using Esba.Application.Common;
using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de mesas de examen (MESAS).</summary>
public interface IMesasQuery
{
    /// <summary>
    /// Listado paginado/filtrado/ordenado para la pantalla "Mesas de examen"
    /// (server-side, §3.2). Join a MATERIAS y MESA_TIPO como el legacy.
    /// </summary>
    Task<PagedResult<MesaListItemDto>> BuscarAsync(MesasFiltro filtro, CancellationToken ct);

    /// <summary>Mesa completa para precargar el formulario de edición; null si no existe.</summary>
    Task<MesaDetailDto?> ObtenerDetalleAsync(string codigoCarrera, int numeroMesa, CancellationToken ct);
}
