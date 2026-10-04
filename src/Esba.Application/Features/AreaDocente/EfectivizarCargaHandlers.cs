using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Common;

namespace Esba.Application.Features.AreaDocente;

/// <summary>
/// Marca la carga de una comisión como EFECTIVIZADA. Solo secretaría, y recién después de
/// haber corrido la regularización real (ConfirmarRegularizacion*Handler, hito 15) con los
/// valores del borrador: este handler no toca CURSADA, solo cierra el circuito de la
/// precarga para que el docente la vea como procesada y deje de aparecer entre las
/// pendientes. Vale desde borrador o finalizada (secretaría puede siempre).
/// </summary>
public sealed class EfectivizarCargaComisionHandler
{
    private readonly ICargaComisionDocenteRepository _cargas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public EfectivizarCargaComisionHandler(ICargaComisionDocenteRepository cargas, IUnitOfWork unitOfWork, TimeProvider reloj)
    {
        _cargas = cargas;
        _unitOfWork = unitOfWork;
        _reloj = reloj;
    }

    public async Task<Result<int>> HandleAsync(CambiarEstadoCargaComisionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var carga = await _cargas.ObtenerPorComisionAsync(command.Comision, ct).ConfigureAwait(false);
        if (carga is null)
        {
            return Result.Error<int>("No hay ninguna precarga para esta comisión.");
        }

        var motivo = AutorizacionCargaDocente.MotivoNoPuedeEfectivizar(command.Actor, carga);
        if (motivo is not null)
        {
            return Result.Error<int>(motivo);
        }

        carga.Efectivizar(command.Actor.CodigoUsuario, _reloj.GetLocalNow().DateTime);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(carga.Id);
    }
}

/// <summary>Marca la carga de una mesa como EFECTIVIZADA tras pasar las notas a los archivos (hito 14). Solo secretaría.</summary>
public sealed class EfectivizarCargaMesaHandler
{
    private readonly ICargaMesaDocenteRepository _cargas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public EfectivizarCargaMesaHandler(ICargaMesaDocenteRepository cargas, IUnitOfWork unitOfWork, TimeProvider reloj)
    {
        _cargas = cargas;
        _unitOfWork = unitOfWork;
        _reloj = reloj;
    }

    public async Task<Result<int>> HandleAsync(CambiarEstadoCargaMesaCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var carga = await _cargas.ObtenerPorMesaAsync(command.Mesa, ct).ConfigureAwait(false);
        if (carga is null)
        {
            return Result.Error<int>("No hay ninguna precarga para esta mesa.");
        }

        var motivo = AutorizacionCargaDocente.MotivoNoPuedeEfectivizar(command.Actor, carga);
        if (motivo is not null)
        {
            return Result.Error<int>(motivo);
        }

        carga.Efectivizar(command.Actor.CodigoUsuario, _reloj.GetLocalNow().DateTime);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(carga.Id);
    }
}
