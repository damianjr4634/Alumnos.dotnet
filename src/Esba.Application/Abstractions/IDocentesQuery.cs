using Esba.Application.Common;
using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de docentes.</summary>
public interface IDocentesQuery
{
    /// <summary>
    /// Docentes activos (FECHA_BAJ IS NULL), ordenados por código — para el combo
    /// de docente del ABM de comisiones (sucesor de cargacomisiones.ComboDocente).
    /// </summary>
    Task<IReadOnlyList<DocenteListItemDto>> ListarActivosAsync(CancellationToken ct);

    /// <summary>Listado paginado/filtrado/ordenado para el ABM de profesores (server-side, §3.2).</summary>
    Task<PagedResult<DocenteListItemDto>> BuscarAsync(DocentesFiltro filtro, CancellationToken ct);

    /// <summary>Docente completo para precargar el formulario de edición; null si no existe.</summary>
    Task<DocenteDetailDto?> ObtenerDetalleAsync(string codigo, CancellationToken ct);
}
