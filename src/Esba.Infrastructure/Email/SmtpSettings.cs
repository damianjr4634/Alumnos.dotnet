namespace Esba.Infrastructure.Email;

/// <summary>
/// Configuración del servidor de correo, cuenta institucional ÚNICA (decisión
/// 2026-06-23). Sucesor del bloque [usuario] de esba_prg.ini + CnfMail.pas, pero
/// global y sin el cifrado casero: se carga por patrón Options desde la sección
/// "Smtp" de appsettings, y las credenciales (User/Password) viven en user-secrets
/// o variables de entorno, nunca en el repo (regla 🔴 §1.3/§2.7).
/// </summary>
public sealed class SmtpSettings
{
    public const string SectionName = "Smtp";

    /// <summary>Servidor SMTP (ej. smtp.gmail.com).</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Puerto SMTP (587 STARTTLS, 465 SSL, 25 sin cifrado).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Dirección remitente institucional (From).</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>Nombre visible del remitente.</summary>
    public string? FromDisplayName { get; set; }

    /// <summary>Usuario de autenticación SMTP. Vacío = sin autenticación. (user-secret)</summary>
    public string? User { get; set; }

    /// <summary>Contraseña de autenticación SMTP. (user-secret)</summary>
    public string? Password { get; set; }

    /// <summary>Modo de seguridad de la conexión (sucesor del combo Autenticación 0/1/2).</summary>
    public SmtpSecurity Security { get; set; } = SmtpSecurity.StartTls;
}

/// <summary>
/// Seguridad de la conexión SMTP. Mapea al combo legacy: 0→None, 1→StartTls,
/// 2→SslOnConnect.
/// </summary>
public enum SmtpSecurity
{
    None,
    StartTls,
    SslOnConnect,
}
