using Esba.Application.Common;
using Esba.Application.DTOs.Administracion;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de usuarios del sistema (USUARIOS) para el ABM.</summary>
public interface IUsuariosQuery
{
    /// <summary>
    /// Listado paginado/filtrado/ordenado para la pantalla "Usuarios"
    /// (server-side, §3.2). Por defecto excluye los dados de baja.
    /// </summary>
    Task<PagedResult<UsuarioListItemDto>> BuscarAsync(UsuariosFiltro filtro, CancellationToken ct);
}
