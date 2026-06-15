using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Inscripción masiva por cuatrimestre (deuda hito 6). Implementa el patrón de
/// dos fases sin transacción de larga vida (§1.3): <see cref="PrevisualizarAsync"/>
/// corre el SP y descarta (rollback) para mostrar qué pasaría;
/// <see cref="ConfirmarAsync"/> lo vuelve a correr y commitea. El SP override de
/// supervisor (FERRCOD=1) llega como NeedsConfirmation: la UI confirma y recién
/// ahí llama a ConfirmarAsync.
/// </summary>
public sealed class InscribirCuatrimestreCompletoHandler
{
    private readonly IInscripcionMasivaCuatrimestreProcedure _procedimiento;
    private readonly ICarrerasQuery _carreras;
    private readonly IValidator<InscribirCuatrimestreCompletoCommand> _validator;

    public InscribirCuatrimestreCompletoHandler(
        IInscripcionMasivaCuatrimestreProcedure procedimiento,
        ICarrerasQuery carreras,
        IValidator<InscribirCuatrimestreCompletoCommand> validator)
    {
        _procedimiento = procedimiento;
        _carreras = carreras;
        _validator = validator;
    }

    /// <summary>Ejecuta el SP y descarta los cambios; devuelve el detalle de lo que ocurriría.</summary>
    public Task<Result<string>> PrevisualizarAsync(InscribirCuatrimestreCompletoCommand command, CancellationToken ct) =>
        EjecutarAsync(command, confirmar: false, ct);

    /// <summary>Re-ejecuta el SP y confirma (commit) la inscripción.</summary>
    public Task<Result<string>> ConfirmarAsync(InscribirCuatrimestreCompletoCommand command, CancellationToken ct) =>
        EjecutarAsync(command, confirmar: true, ct);

    private async Task<Result<string>> EjecutarAsync(
        InscribirCuatrimestreCompletoCommand command, bool confirmar, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var datos = await _carreras.ObtenerDatosInscripcionAsync(command.CodigoCarrera, ct).ConfigureAwait(false);

        var parametros = new InscripcionMasivaParametros
        {
            CodigoAlumno = command.CodigoAlumno,
            Curso = command.Curso,
            CodigoCarrera = command.CodigoCarrera,
            CuatrimestreAnio = command.CuatrimestreAnio,
            Instituto = datos?.Instituto,
            Caracteristica = datos?.Caracteristica,
            CodigoUsuario = command.CodigoUsuario,
        };

        return await _procedimiento.EjecutarAsync(parametros, confirmar, ct).ConfigureAwait(false);
    }
}
