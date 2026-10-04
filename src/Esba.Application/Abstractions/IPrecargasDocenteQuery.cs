using Esba.Application.Common;
using Esba.Application.DTOs.AreaDocente;

namespace Esba.Application.Abstractions;

/// <summary>Listados de precargas de docentes para secretaría (hito 19): qué hay para revisar y efectivizar.</summary>
public interface IPrecargasDocenteQuery
{
    /// <summary>Cargas por comisión, server-side; las finalizadas primero (son las pendientes).</summary>
    Task<PagedResult<PrecargaComisionListItemDto>> BuscarComisionesAsync(PrecargasDocenteFiltro filtro, CancellationToken ct);

    /// <summary>Cargas por mesa, server-side; las finalizadas primero.</summary>
    Task<PagedResult<PrecargaMesaListItemDto>> BuscarMesasAsync(PrecargasDocenteFiltro filtro, CancellationToken ct);
}
