using Esba.Application.Common;
using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de comisiones armadas (COMARM).</summary>
public interface IComisionesQuery
{
    /// <summary>
    /// Listado paginado/filtrado/ordenado para la pantalla "Listado de comisiones"
    /// (server-side, §3.2). Hace el join a MATERIAS y DOCENTES como el legacy.
    /// </summary>
    Task<PagedResult<ComisionListItemDto>> BuscarAsync(ComisionesFiltro filtro, CancellationToken ct);
}
