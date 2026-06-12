namespace Esba.Application.Features.Administracion;

/// <summary>Login (sucesor de sesion.pas).</summary>
public sealed record IniciarSesionCommand
{
    public required string NombreUsuario { get; init; }

    public required string Password { get; init; }
}
