using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Elimina la cursada de una materia (sucesor del DELETE de
/// eliminacursadaClick). La confirmación es de la UI; el trigger limpia
/// RECURSA si correspondía.
/// </summary>
public sealed class EliminarInscripcionHandler
{
    private readonly ICursadaRepository _cursadas;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarInscripcionHandler(ICursadaRepository cursadas, IUnitOfWork unitOfWork)
    {
        _cursadas = cursadas;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(EliminarInscripcionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var cursada = await _cursadas.ObtenerAsync(command.CodigoCarrera, command.CodigoAlumno, command.CodigoMateria, ct).ConfigureAwait(false);
        if (cursada is null)
        {
            return Result.Error<string>("La inscripción no existe.");
        }

        _cursadas.Eliminar(cursada);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(command.CodigoMateria);
    }
}
