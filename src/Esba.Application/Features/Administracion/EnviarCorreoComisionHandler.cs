using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Envía un correo a los alumnos de una comisión (sucesor de enviarClick de
/// enviocorreo.pas): un único mensaje con todos los destinatarios y, si está
/// configurada en XXX_CONF, una copia interna de auditoría. No toca la base salvo
/// la lectura de los parámetros Mail_EnvCopia/CC/CCO.
/// </summary>
public sealed class EnviarCorreoComisionHandler
{
    private const string ParamCopia = "Mail_EnvCopia";
    private const string ParamCopiaCc = "Mail_EnvCopiaCC";
    private const string ParamCopiaCco = "Mail_EnvCopiaCCO";

    private readonly IEmailService _email;
    private readonly IConfiguracionQuery _configuracion;
    private readonly IValidator<EnviarCorreoComisionCommand> _validator;

    public EnviarCorreoComisionHandler(
        IEmailService email,
        IConfiguracionQuery configuracion,
        IValidator<EnviarCorreoComisionCommand> validator)
    {
        _email = email;
        _configuracion = configuracion;
        _validator = validator;
    }

    public async Task<Result<bool>> HandleAsync(EnviarCorreoComisionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<bool>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var mensaje = new MensajeCorreo
        {
            Para = command.Para,
            CopiaCarbon = command.CopiaCarbon,
            CopiaOculta = command.CopiaOculta,
            Asunto = command.Asunto,
            Cuerpo = command.Cuerpo,
        };

        var envio = await _email.EnviarAsync(mensaje, ct).ConfigureAwait(false);
        if (!envio.IsSuccess)
        {
            return envio;
        }

        // Copia interna de auditoría a las direcciones configuradas en XXX_CONF.
        var copia = await EnviarCopiaAuditoriaAsync(command, ct).ConfigureAwait(false);
        if (copia is { IsSuccess: false })
        {
            return Result.Warning(true,
                "El correo se envió, pero falló la copia de auditoría: " + copia.Message);
        }

        return Result.Ok(true);
    }

    private async Task<Result<bool>?> EnviarCopiaAuditoriaAsync(
        EnviarCorreoComisionCommand command, CancellationToken ct)
    {
        var copia = (await _configuracion.ObtenerValorAsync(ParamCopia, ct).ConfigureAwait(false))?.Trim();
        if (string.IsNullOrWhiteSpace(copia))
        {
            return null;
        }

        var cc = (await _configuracion.ObtenerValorAsync(ParamCopiaCc, ct).ConfigureAwait(false))?.Trim();
        var cco = (await _configuracion.ObtenerValorAsync(ParamCopiaCco, ct).ConfigureAwait(false))?.Trim();

        var cuerpo = string.Join(Environment.NewLine,
            $"El Usuario {command.UsuarioRemitente} envió este correo el día de la fecha.",
            "Direcciones de destino:",
            "Para: " + string.Join("; ", command.Para),
            "Copia: " + string.Join("; ", command.CopiaCarbon),
            "CCO: " + string.Join("; ", command.CopiaOculta),
            "Asunto del mensaje: " + command.Asunto,
            "Cuerpo del mensaje:",
            command.Cuerpo);

        var auditoria = new MensajeCorreo
        {
            Para = [copia],
            CopiaCarbon = string.IsNullOrWhiteSpace(cc) ? [] : [cc],
            CopiaOculta = string.IsNullOrWhiteSpace(cco) ? [] : [cco],
            Asunto = $"SISTEMA DE ALUMNO - Envio de correo por el usuario {command.UsuarioRemitente}",
            Cuerpo = cuerpo,
        };

        return await _email.EnviarAsync(auditoria, ct).ConfigureAwait(false);
    }
}
