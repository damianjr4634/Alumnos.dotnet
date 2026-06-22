using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface IUsuarioRepository
{
    /// <summary>Busca por nombre de login (NOMBRE) con sus permisos de BARRA_SEGU, trackeado para poder persistir re-hash y UID de sesión.</summary>
    Task<Usuario?> ObtenerPorNombreConPermisosAsync(string nombreUsuario, CancellationToken ct);

    /// <summary>Busca por PK (CODUSU), trackeado para edición/baja/blanqueo.</summary>
    Task<Usuario?> ObtenerPorCodigoAsync(int codigo, CancellationToken ct);

    /// <summary>true si ya existe un usuario con ese nombre de login (comparación insensible a mayúsculas), opcionalmente excluyendo un código (para validar unicidad al editar).</summary>
    Task<bool> ExisteNombreAsync(string nombreUsuario, int? codigoExcluido, CancellationToken ct);

    /// <summary>Cantidad de supervisores activos (SUPERV='S' y FECHA_BAJ nula): evita dejar el sistema sin administrador.</summary>
    Task<int> ContarSupervisoresActivosAsync(CancellationToken ct);

    void Agregar(Usuario usuario);
}
