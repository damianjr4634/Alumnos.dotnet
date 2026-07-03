using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Eliminación de un año lectivo de TBL_CUAT o TBL_TRIM (en el legacy se
/// borraba la fila de la grilla en memoria y el grabado reinsertaba el resto).
/// </summary>
public sealed class EliminarCicloLectivoHandler
{
    private readonly ICicloLectivoRepository _ciclos;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarCicloLectivoHandler(ICicloLectivoRepository ciclos, IUnitOfWork unitOfWork)
    {
        _ciclos = ciclos;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(EliminarCicloLectivoCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Trimestral)
        {
            var trimestral = await _ciclos.ObtenerTrimestralAsync(command.Anio, ct).ConfigureAwait(false);
            if (trimestral is null)
            {
                return Result.Error<int>($"El año {command.Anio} no existe.");
            }

            _ciclos.Eliminar(trimestral);
        }
        else
        {
            var cuatrimestral = await _ciclos.ObtenerCuatrimestralAsync(command.Anio, ct).ConfigureAwait(false);
            if (cuatrimestral is null)
            {
                return Result.Error<int>($"El año {command.Anio} no existe.");
            }

            _ciclos.Eliminar(cuatrimestral);
        }

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result.Ok(command.Anio);
    }
}
