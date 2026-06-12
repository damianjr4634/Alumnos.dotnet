using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface IUsuarioRepository
{
    /// <summary>Busca por nombre de login (NOMBRE) con sus permisos de BARRA_SEGU, trackeado para poder persistir re-hash y UID de sesión.</summary>
    Task<Usuario?> ObtenerPorNombreConPermisosAsync(string nombreUsuario, CancellationToken ct);
}
