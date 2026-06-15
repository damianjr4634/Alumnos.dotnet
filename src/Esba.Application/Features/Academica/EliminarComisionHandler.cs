using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;

namespace Esba.Application.Features.Academica;

/// <summary>Baja de comisión (sucesor de cargacomisiones.EliminarClick: DELETE simple).</summary>
public sealed class EliminarComisionHandler
{
    private readonly IComisionRepository _comisiones;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarComisionHandler(IComisionRepository comisiones, IUnitOfWork unitOfWork)
    {
        _comisiones = comisiones;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(EliminarComisionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var comision = await _comisiones.ObtenerAsync(
            command.CodigoCarrera, command.Cutuco, command.CodigoMateria, command.CuatrimestreAnio, ct)
            .ConfigureAwait(false);
        if (comision is null)
        {
            return Result.Error<string>("La comisión no existe.");
        }

        _comisiones.Eliminar(comision);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(command.CodigoMateria);
    }
}
