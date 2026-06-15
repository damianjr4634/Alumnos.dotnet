using Esba.Application.Common;
using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas del catálogo de materias.</summary>
public interface IMateriasQuery
{
    /// <summary>Listado completo por carrera (lo usa la inscripción de cursada).</summary>
    Task<IReadOnlyList<MateriaListItemDto>> ListarPorCarreraAsync(string codigoCarrera, CancellationToken ct);

    /// <summary>
    /// Listado paginado/filtrado/ordenado para la pantalla "Listado de materias"
    /// (server-side: paginación y orden resueltos en Firebird, §3.2).
    /// </summary>
    Task<PagedResult<MateriaListItemDto>> BuscarAsync(MateriasFiltro filtro, CancellationToken ct);
}
