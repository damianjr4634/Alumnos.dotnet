using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Examenes;

/// <summary>
/// Alta de mesa de examen (sucesor del INSERT de MesasExamen.GrabamesaClick).
/// Pre-chequea duplicado con XXX_VALIDO_MESA antes de insertar por EF.
/// </summary>
public sealed class CrearMesaHandler
{
    private readonly IMesaRepository _mesas;
    private readonly IValidoMesaProcedure _validoMesa;
    private readonly IValidator<CrearMesaCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CrearMesaHandler(
        IMesaRepository mesas,
        IValidoMesaProcedure validoMesa,
        IValidator<CrearMesaCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _mesas = mesas;
        _validoMesa = validoMesa;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(CrearMesaCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var duplicado = await _validoMesa.VerificarAsync(command.NumeroMesa, command.CodigoCarrera, ct).ConfigureAwait(false);
        if (duplicado.Status == OperationStatus.Error)
        {
            return Result.Error<int>(duplicado.Message ?? "La mesa ya existe.");
        }

        var mesa = new Mesa { CodigoCarrera = command.CodigoCarrera, NumeroMesa = command.NumeroMesa };
        MesaMapping.Aplicar(mesa, command, usuario);

        _mesas.Agregar(mesa);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(command.NumeroMesa);
    }
}
