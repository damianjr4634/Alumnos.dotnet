namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Reemplaza el set completo de permisos de un usuario (sucesor de
/// PermisosPorUsuario.pas sobre BARRA_SEGU).
/// </summary>
public sealed record AsignarPermisosUsuarioCommand
{
    public required int CodigoUsuario { get; init; }

    /// <summary>Códigos de carrera/opción habilitados (BAROPC).</summary>
    public required IReadOnlyList<string> CodigosOpcion { get; init; }
}
