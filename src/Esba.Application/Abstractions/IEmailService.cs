using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Envío de correo (sucesor de enviocorreo.pas + Indy TIdSMTP). La implementación
/// usa MailKit y la cuenta institucional configurada (§3.5). Las fallas viajan como
/// <see cref="Result{T}"/>, no como excepción a la UI (§2.5).
/// </summary>
public interface IEmailService
{
    Task<Result<bool>> EnviarAsync(MensajeCorreo mensaje, CancellationToken ct);
}
