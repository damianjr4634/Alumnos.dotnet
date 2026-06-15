using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Modificación de comisión (sucesor de cargacomisiones.GrabaMateriaClick en
/// modificación). La clave no cambia; se editan docente, horario y
/// titular/suplente, revalidando con XXX_VAL_COMISION en la misma transacción.
/// </summary>
public sealed class ActualizarComisionHandler
{
    private readonly IComisionRepository _comisiones;
    private readonly IValidator<ActualizarComisionCommand> _validator;

    public ActualizarComisionHandler(
        IComisionRepository comisiones,
        IValidator<ActualizarComisionCommand> validator)
    {
        _comisiones = comisiones;
        _validator = validator;
    }

    public async Task<Result<string>> HandleAsync(ActualizarComisionCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var comision = await _comisiones.ObtenerAsync(
            command.CodigoCarrera, command.Cutuco, command.CodigoMateria, command.CuatrimestreAnio, ct)
            .ConfigureAwait(false);
        if (comision is null)
        {
            return Result.Error<string>("La comisión no existe.");
        }

        comision.CodigoProfesor = command.CodigoProfesor;
        comision.TitularSuplente = command.EsTitular ? "T" : "S";
        comision.Usuario = usuario;
        ComisionMapping.AplicarHorario(comision, command.Horario);

        return await _comisiones.GuardarYValidarAsync(comision, esAlta: false, ct).ConfigureAwait(false);
    }
}
