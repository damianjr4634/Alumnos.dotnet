using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Esba.Infrastructure.Email;

/// <summary>
/// Envío de correo con MailKit usando la cuenta institucional configurada
/// (<see cref="SmtpSettings"/>). Reemplaza Indy TIdSMTP + OpenSSL de enviocorreo.pas.
/// Toda falla (config incompleta, conexión, autenticación, dirección inválida) se
/// devuelve como Result.Error con mensaje amigable y se loguea; nunca se propaga la
/// excepción a la UI (§2.5).
/// </summary>
public sealed class MailKitEmailService : IEmailService
{
    private static readonly Action<ILogger, string, Exception?> LogFalloEnvio =
        LoggerMessage.Define<string>(
            LogLevel.Error, new EventId(1, nameof(MailKitEmailService)),
            "Fallo al enviar correo a {Destinatarios}.");

    private readonly SmtpSettings _settings;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IOptions<SmtpSettings> options, ILogger<MailKitEmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<Result<bool>> EnviarAsync(MensajeCorreo mensaje, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(mensaje);

        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.From))
        {
            return Result.Error<bool>(
                "El correo no está configurado. Definí Host y remitente (From) en la sección Smtp.");
        }

        if (mensaje.Para.Count == 0)
        {
            return Result.Error<bool>("El mensaje no tiene destinatarios.");
        }

        try
        {
            var mime = ConstruirMime(mensaje);

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, MapearSeguridad(_settings.Security), ct)
                .ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(_settings.User))
            {
                await client.AuthenticateAsync(_settings.User, _settings.Password ?? string.Empty, ct)
                    .ConfigureAwait(false);
            }

            await client.SendAsync(mime, ct).ConfigureAwait(false);
            await client.DisconnectAsync(quit: true, ct).ConfigureAwait(false);

            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            LogFalloEnvio(_logger, string.Join(", ", mensaje.Para), ex);
            return Result.Error<bool>($"No se pudo enviar el correo: {ex.Message}");
        }
    }

    private MimeMessage ConstruirMime(MensajeCorreo mensaje)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_settings.FromDisplayName ?? string.Empty, _settings.From));

        foreach (var destino in mensaje.Para)
        {
            mime.To.Add(MailboxAddress.Parse(destino));
        }

        foreach (var copia in mensaje.CopiaCarbon)
        {
            mime.Cc.Add(MailboxAddress.Parse(copia));
        }

        foreach (var copia in mensaje.CopiaOculta)
        {
            mime.Bcc.Add(MailboxAddress.Parse(copia));
        }

        mime.Subject = mensaje.Asunto;

        var cuerpo = new BodyBuilder();
        if (mensaje.EsHtml)
        {
            cuerpo.HtmlBody = mensaje.Cuerpo;
        }
        else
        {
            cuerpo.TextBody = mensaje.Cuerpo;
        }

        foreach (var adjunto in mensaje.Adjuntos)
        {
            cuerpo.Attachments.Add(adjunto.NombreArchivo, adjunto.Contenido, ContentType.Parse(adjunto.TipoContenido));
        }

        mime.Body = cuerpo.ToMessageBody();
        return mime;
    }

    private static SecureSocketOptions MapearSeguridad(SmtpSecurity seguridad) => seguridad switch
    {
        SmtpSecurity.None => SecureSocketOptions.None,
        SmtpSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
        _ => SecureSocketOptions.StartTls,
    };
}
