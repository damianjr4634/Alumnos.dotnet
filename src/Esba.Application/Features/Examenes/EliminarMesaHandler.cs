using Esba.Application.Abstractions;
using Esba.Domain.Common;

namespace Esba.Application.Features.Examenes;

/// <summary>Baja de mesa de examen (sucesor de MesasExamen.eliminaMesaClick: DELETE simple).</summary>
public sealed class EliminarMesaHandler
{
    private readonly IMesaRepository _mesas;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarMesaHandler(IMesaRepository mesas, IUnitOfWork unitOfWork)
    {
        _mesas = mesas;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(string codigoCarrera, int numeroMesa, CancellationToken ct)
    {
        var mesa = await _mesas.ObtenerAsync(codigoCarrera, numeroMesa, ct).ConfigureAwait(false);
        if (mesa is null)
        {
            return Result.Error<int>("La mesa no existe.");
        }

        _mesas.Eliminar(mesa);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(numeroMesa);
    }
}
