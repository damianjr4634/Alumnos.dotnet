namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Alta de usuario (sucesor del INSERT de AltaUsuario.pas). CODUSU lo genera el
/// trigger; SUPERV/CAMPASS nacen 'N' por trigger, como en el legacy. La
/// contraseña viaja en claro hasta el caso de uso, que la hashea (PBKDF2,
/// migration_improvements.md §2.7) — nunca se persiste en claro.
/// </summary>
public sealed record CrearUsuarioCommand
{
    public required string NombreUsuario { get; init; }

    public required string Password { get; init; }

    public string? Nombres { get; init; }

    public string? Apellido { get; init; }

    public string? Cargo { get; init; }
}
