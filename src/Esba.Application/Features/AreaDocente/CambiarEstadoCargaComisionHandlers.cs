using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Common;
using Esba.Domain.Enums;

namespace Esba.Application.Features.AreaDocente;

/// <summary>
/// Finaliza la carga de una comisión (BOR → FIN): el docente titular (o secretaría) da
/// por terminada la precarga; desde ahí el docente solo lee. Sin validador propio: la
/// única entrada es la clave de la comisión y el actor, y las reglas están en
/// <see cref="AutorizacionCargaDocente"/>.
/// </summary>
public sealed class FinalizarCargaComisionHandler
{
    private readonly ICargaComisionDocenteRepository _cargas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public FinalizarCargaComisionHandler(ICargaComisionDocenteRepository cargas, IUnitOfWork unitOfWork, TimeProvider reloj)
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
            return Result.Error<int>("Todavía no hay nada cargado para esta comisión: guardá el borrador antes de finalizar.");
        }

        var motivo = AutorizacionCargaDocente.MotivoNoPuedeFinalizar(command.Actor, carga);
        if (motivo is not null)
        {
            return Result.Error<int>(motivo);
        }

        carga.Finalizar(command.Actor.CodigoUsuario, _reloj.GetLocalNow().DateTime);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(carga.Id);
    }
}

/// <summary>
/// Reabre una carga (FIN/EFE → BOR) para que el docente pueda seguir cargando. Solo
/// secretaría. Reabrir una carga ya efectivizada se permite (secretaría puede siempre)
/// pero se avisa: los valores ya copiados a CURSADA no se deshacen.
/// </summary>
public sealed class ReabrirCargaComisionHandler
{
    private readonly ICargaComisionDocenteRepository _cargas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public ReabrirCargaComisionHandler(ICargaComisionDocenteRepository cargas, IUnitOfWork unitOfWork, TimeProvider reloj)
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
            return Result.Error<int>("No hay ninguna carga para esta comisión.");
        }

        var motivo = AutorizacionCargaDocente.MotivoNoPuedeReabrir(command.Actor, carga);
        if (motivo is not null)
        {
            return Result.Error<int>(motivo);
        }

        var estabaEfectivizada = carga.Estado == EstadoCargaDocente.Efectivizada;
        carga.Reabrir(command.Actor.CodigoUsuario, _reloj.GetLocalNow().DateTime);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return estabaEfectivizada
            ? Result.Warning(carga.Id, "La carga estaba efectivizada: lo ya volcado a CURSADA no se deshace; al volver a efectivizar se pisará con los nuevos valores.")
            : Result.Ok(carga.Id);
    }
}
