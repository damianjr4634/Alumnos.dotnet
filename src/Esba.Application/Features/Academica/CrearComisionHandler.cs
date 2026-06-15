using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Alta de comisión (sucesor de cargacomisiones.GrabaMateriaClick en alta).
/// Pre-chequea duplicado con XXX_VALIDO_COMISION y luego inserta validando el
/// horario con XXX_VAL_COMISION en la misma transacción (rollback si falla).
/// </summary>
public sealed class CrearComisionHandler
{
    private readonly IComisionRepository _comisiones;
    private readonly IValidoComisionProcedure _validoComision;
    private readonly IValidator<CrearComisionCommand> _validator;

    public CrearComisionHandler(
        IComisionRepository comisiones,
        IValidoComisionProcedure validoComision,
        IValidator<CrearComisionCommand> validator)
    {
        _comisiones = comisiones;
        _validoComision = validoComision;
        _validator = validator;
    }

    public async Task<Result<string>> HandleAsync(CrearComisionCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        // Pre-chequeo de duplicado (XXX_VALIDO_COMISION).
        var duplicado = await _validoComision.VerificarAsync(
            command.CuatrimestreAnio, command.CodigoMateria, command.Cutuco, command.CodigoCarrera, ct)
            .ConfigureAwait(false);
        if (duplicado.Status == OperationStatus.Error)
        {
            return Result.Error<string>(duplicado.Message ?? "La comisión ya existe.");
        }

        var comision = new Comision
        {
            CodigoCarrera = command.CodigoCarrera,
            Cutuco = command.Cutuco,
            CodigoMateria = command.CodigoMateria,
            CuatrimestreAnio = command.CuatrimestreAnio,
            CodigoProfesor = command.CodigoProfesor,
            TitularSuplente = command.EsTitular ? "T" : "S",
            Usuario = usuario,
        };
        ComisionMapping.AplicarHorario(comision, command.Horario);

        return await _comisiones.GuardarYValidarAsync(comision, esAlta: true, ct).ConfigureAwait(false);
    }
}
