namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Cambio de contraseña por el propio usuario (sucesor de CambioPassword.pas).
/// Verifica la clave actual y deja CAMPASS='N'. Las contraseñas viajan en claro
/// hasta el caso de uso, que hashea con PBKDF2 (§2.7) — nunca se persisten en claro.
/// </summary>
public sealed record CambiarPasswordCommand
{
    public required int CodigoUsuario { get; init; }

    public required string PasswordActual { get; init; }

    public required string PasswordNueva { get; init; }

    public required string PasswordNuevaConfirmacion { get; init; }
}
