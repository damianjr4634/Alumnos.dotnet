using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Alumnos;

/// <summary>
/// Elimina físicamente un alumno con todo su historial vía XXX_BORRA_ALUMNO (sucesor
/// de dxBarButton43Click de FrmEsba.pas), con el patrón de dos fases: previsualizar
/// (rollback, devuelve la advertencia del SP) y confirmar (commit). Solo supervisores:
/// se corta acá con el claim antes de tocar la base, además del control que el SP
/// hace sobre USUARIOS.SUPERV.
/// </summary>
public sealed class BorrarAlumnoHandler
{
    public const string MensajeSoloSupervisor = "Solo un supervisor puede borrar a un alumno. Avise a rectoría.";

    private readonly IValidator<BorrarAlumnoCommand> _validator;
    private readonly IBorraAlumnoProcedure _borra;

    public BorrarAlumnoHandler(IValidator<BorrarAlumnoCommand> validator, IBorraAlumnoProcedure borra)
    {
        _validator = validator;
        _borra = borra;
    }

    public Task<Result<string>> PrevisualizarAsync(BorrarAlumnoCommand command, CancellationToken ct) =>
        EjecutarAsync(command, confirmar: false, ct);

    public Task<Result<string>> ConfirmarAsync(BorrarAlumnoCommand command, CancellationToken ct) =>
        EjecutarAsync(command, confirmar: true, ct);

    private async Task<Result<string>> EjecutarAsync(BorrarAlumnoCommand command, bool confirmar, CancellationToken ct)
    {
        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(validacion.Errors[0].ErrorMessage);
        }

        if (!command.EsSupervisor)
        {
            return Result.Error<string>(MensajeSoloSupervisor);
        }

        return await _borra.EjecutarAsync(
            command.CodigoCarrera.Trim(), command.CodigoAlumno.Trim(), command.CodigoUsuario, confirmar, ct)
            .ConfigureAwait(false);
    }
}
