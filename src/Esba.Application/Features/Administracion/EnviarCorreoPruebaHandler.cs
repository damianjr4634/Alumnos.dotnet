using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Envía un correo de prueba para validar la configuración SMTP de punta a punta.
/// No toca la base: solo compone un mensaje fijo y delega en <see cref="IEmailService"/>.
/// </summary>
public sealed class EnviarCorreoPruebaHandler
{
    private readonly IEmailService _email;
    private readonly IValidator<EnviarCorreoPruebaCommand> _validator;

    public EnviarCorreoPruebaHandler(IEmailService email, IValidator<EnviarCorreoPruebaCommand> validator)
    {
        _email = email;
        _validator = validator;
    }

    public async Task<Result<bool>> HandleAsync(EnviarCorreoPruebaCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<bool>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var mensaje = new MensajeCorreo
        {
            Para = [command.Destinatario.Trim()],
            Asunto = "ESBA — Correo de prueba",
            Cuerpo = "Este es un correo de prueba del sistema ESBA. "
                + "Si lo recibiste, la configuración de correo (SMTP) es correcta.",
            EsHtml = false,
        };

        return await _email.EnviarAsync(mensaje, ct).ConfigureAwait(false);
    }
}
