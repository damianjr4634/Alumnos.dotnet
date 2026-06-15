using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Examenes;

/// <summary>Modificación de mesa de examen (sucesor del UPDATE de MesasExamen.GrabamesaClick).</summary>
public sealed class ActualizarMesaHandler
{
    private readonly IMesaRepository _mesas;
    private readonly IValidator<ActualizarMesaCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarMesaHandler(
        IMesaRepository mesas,
        IValidator<ActualizarMesaCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _mesas = mesas;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(ActualizarMesaCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var mesa = await _mesas.ObtenerAsync(command.CodigoCarrera, command.NumeroMesa, ct).ConfigureAwait(false);
        if (mesa is null)
        {
            return Result.Error<int>("La mesa no existe.");
        }

        MesaMapping.Aplicar(mesa, command, usuario);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(command.NumeroMesa);
    }
}
