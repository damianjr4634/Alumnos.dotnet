namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Blanqueo de contraseña por un administrador (sucesor del blanqueo legacy
/// PASSWD='/' + CAMPASS='S', adaptado a PBKDF2): el admin fija una clave temporal
/// y el usuario queda obligado a cambiarla en su próximo login (CAMPASS='S').
/// </summary>
public sealed record BlanquearPasswordCommand
{
    public required int CodigoUsuario { get; init; }

    public required string PasswordTemporal { get; init; }
}
