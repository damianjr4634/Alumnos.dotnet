using Esba.Application.Abstractions;
using Esba.Application.DTOs.Alumnos;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Alumnos;

/// <summary>
/// Copia o mueve un alumno a otra carrera vía XXX_COPIA_ALUMNO / XXX_MUEVE_ALUMNO
/// (sucesor de dxBarButton29Click / dxBarButton30Click de FrmEsba.pas), con el patrón
/// de dos fases: previsualizar (el SP ejecuta y se hace rollback; devuelve el mensaje
/// de confirmación o el error) y confirmar (commit).
/// </summary>
public sealed class CambiarCarreraAlumnoHandler
{
    private readonly IValidator<CambiarCarreraAlumnoCommand> _validator;
    private readonly ICopiaAlumnoProcedure _copia;
    private readonly IMueveAlumnoProcedure _mueve;

    public CambiarCarreraAlumnoHandler(
        IValidator<CambiarCarreraAlumnoCommand> validator,
        ICopiaAlumnoProcedure copia,
        IMueveAlumnoProcedure mueve)
    {
        _validator = validator;
        _copia = copia;
        _mueve = mueve;
    }

    public Task<Result<string>> PrevisualizarCopiaAsync(CambiarCarreraAlumnoCommand command, CancellationToken ct) =>
        EjecutarAsync(command, mover: false, confirmar: false, ct);

    public Task<Result<string>> ConfirmarCopiaAsync(CambiarCarreraAlumnoCommand command, CancellationToken ct) =>
        EjecutarAsync(command, mover: false, confirmar: true, ct);

    public Task<Result<string>> PrevisualizarMovimientoAsync(CambiarCarreraAlumnoCommand command, CancellationToken ct) =>
        EjecutarAsync(command, mover: true, confirmar: false, ct);

    public Task<Result<string>> ConfirmarMovimientoAsync(CambiarCarreraAlumnoCommand command, CancellationToken ct) =>
        EjecutarAsync(command, mover: true, confirmar: true, ct);

    private async Task<Result<string>> EjecutarAsync(
        CambiarCarreraAlumnoCommand command, bool mover, bool confirmar, CancellationToken ct)
    {
        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(validacion.Errors[0].ErrorMessage);
        }

        var parametros = new CambioCarreraAlumnoParametros
        {
            CodigoAlumno = command.CodigoAlumno.Trim(),
            CodigoCarreraOrigen = command.CodigoCarreraOrigen.Trim(),
            CodigoCarreraDestino = command.CodigoCarreraDestino.Trim(),
            CodigoUsuario = command.CodigoUsuario,
        };

        return mover
            ? await _mueve.EjecutarAsync(parametros, confirmar, ct).ConfigureAwait(false)
            : await _copia.EjecutarAsync(parametros, confirmar, ct).ConfigureAwait(false);
    }
}
