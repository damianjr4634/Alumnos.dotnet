using Esba.Application.Abstractions;
using Esba.Domain.Common;

namespace Esba.Application.Features.Administracion;

/// <summary>Reactiva un docente dado de baja (FECHA_BAJ = NULL). Inversa de <see cref="DarDeBajaDocenteHandler"/>.</summary>
public sealed class ReactivarDocenteHandler
{
    private readonly IDocenteRepository _docentes;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivarDocenteHandler(IDocenteRepository docentes, IUnitOfWork unitOfWork)
    {
        _docentes = docentes;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(string codigo, CancellationToken ct)
    {
        var docente = await _docentes.ObtenerPorCodigoAsync(codigo.Trim(), ct).ConfigureAwait(false);
        if (docente is null)
        {
            return Result.Error<string>("El docente no existe.");
        }

        if (!docente.EstaDeBaja)
        {
            return Result.Warning(docente.Codigo, "El docente ya estaba activo.");
        }

        docente.FechaBaja = null;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(docente.Codigo);
    }
}
