namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Modificación de datos del usuario. La contraseña NO se cambia por acá: tiene
/// sus propios flujos (cambio por el propio usuario con CAMPASS, y blanqueo por
/// un administrador), como en el legacy (CambioPassword.pas / FrmEsba).
/// </summary>
public sealed record ActualizarUsuarioCommand
{
    public required int Codigo { get; init; }

    public required string NombreUsuario { get; init; }

    public string? Nombres { get; init; }

    public string? Apellido { get; init; }

    public string? Cargo { get; init; }

    public bool EsSupervisor { get; init; }
}
