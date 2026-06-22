using Esba.Application.DTOs.Administracion;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de YYY_SEGU_OPCIONES: lista todas las carreras + opciones de menú con
/// su estado (habilitada/no) para un usuario, fuente del diálogo de permisos.
/// </summary>
public interface ISeguOpcionesProcedure
{
    Task<IReadOnlyList<PermisoOpcionDto>> ListarAsync(int codigoUsuario, CancellationToken ct);
}
