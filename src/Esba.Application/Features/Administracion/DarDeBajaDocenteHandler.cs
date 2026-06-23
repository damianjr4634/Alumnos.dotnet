using Esba.Application.Abstractions;
using Esba.Domain.Common;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Baja lógica de un docente (FECHA_BAJ = hoy). El formulario legacy ElimProfes
/// estaba inactivo; este comportamiento es nuevo (hito 10.2). Un docente dado de
/// baja deja de aparecer en los combos de comisiones/mesas (filtran FECHA_BAJ IS NULL).
/// </summary>
public sealed class DarDeBajaDocenteHandler
{
    private readonly IDocenteRepository _docentes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public DarDeBajaDocenteHandler(IDocenteRepository docentes, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _docentes = docentes;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<string>> HandleAsync(string codigo, CancellationToken ct)
    {
        var docente = await _docentes.ObtenerPorCodigoAsync(codigo.Trim(), ct).ConfigureAwait(false);
        if (docente is null)
        {
            return Result.Error<string>("El docente no existe.");
        }

        if (docente.EstaDeBaja)
        {
            return Result.Warning(docente.Codigo, "El docente ya estaba dado de baja.");
        }

        docente.FechaBaja = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(docente.Codigo);
    }
}
