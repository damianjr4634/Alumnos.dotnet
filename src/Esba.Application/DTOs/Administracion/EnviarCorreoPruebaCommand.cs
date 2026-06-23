namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Envía un correo de prueba a una dirección para verificar la configuración SMTP
/// institucional (sucesor de la prueba manual del legacy desde CnfMail).
/// </summary>
public sealed record EnviarCorreoPruebaCommand
{
    public required string Destinatario { get; init; }
}
