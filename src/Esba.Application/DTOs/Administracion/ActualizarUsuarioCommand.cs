using Esba.Domain.Enums;

namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Modificación de datos del usuario. La contraseña NO se cambia por acá: tiene
/// sus propios flujos (cambio por el propio usuario con CAMPASS, y blanqueo por
/// un administrador), como en el legacy (CambioPassword.pas / FrmEsba).
/// El tipo y el vínculo también se editan acá; cambiar el tipo tiene efectos
/// sobre PASSWD (ver ActualizarUsuarioHandler).
/// </summary>
public sealed record ActualizarUsuarioCommand
{
    public required int Codigo { get; init; }

    public required string NombreUsuario { get; init; }

    public string? Nombres { get; init; }

    public string? Apellido { get; init; }

    public string? Cargo { get; init; }

    public bool EsSupervisor { get; init; }

    public TipoUsuario Tipo { get; init; } = TipoUsuario.Secretaria;

    public string? CodigoDocente { get; init; }

    public string? AlumnoCarrera { get; init; }

    public string? AlumnoCodigo { get; init; }
}
