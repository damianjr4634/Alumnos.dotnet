namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Alta de usuario (sucesor del INSERT de AltaUsuario.pas). CODUSU lo genera el
/// trigger. La contraseña viaja en claro hasta el caso de uso, que la hashea
/// (PBKDF2, migration_improvements.md §2.7) — nunca se persiste en claro. El
/// alta nace con CAMPASS='S' (cambio forzado en el primer login): el admin pone
/// una clave inicial y el usuario la cambia (mejora sobre el legacy, que nacía 'N').
/// </summary>
public sealed record CrearUsuarioCommand
{
    public required string NombreUsuario { get; init; }

    public required string Password { get; init; }

    public string? Nombres { get; init; }

    public string? Apellido { get; init; }

    public string? Cargo { get; init; }

    /// <summary>SUPERV: ve todas las carreras sin filtro BARRA_SEGU y administra el sistema.</summary>
    public bool EsSupervisor { get; init; }
}
