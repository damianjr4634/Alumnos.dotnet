using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Common;
using Esba.Domain.Enums;

namespace Esba.Application.Features.AreaDocente;

/// <summary>Finaliza la carga de una mesa (BOR → FIN): el titular (o secretaría) da por terminada la precarga.</summary>
public sealed class FinalizarCargaMesaHandler
{
    private readonly ICargaMesaDocenteRepository _cargas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public FinalizarCargaMesaHandler(ICargaMesaDocenteRepository cargas, IUnitOfWork unitOfWork, TimeProvider reloj)
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
            return Result.Error<int>("Todavía no hay nada cargado para esta mesa: guardá el borrador antes de finalizar.");
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

/// <summary>Reabre la carga de una mesa (FIN/EFE → BOR). Solo secretaría; desde EFE avisa que lo ya volcado no se deshace.</summary>
public sealed class ReabrirCargaMesaHandler
{
    private readonly ICargaMesaDocenteRepository _cargas;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _reloj;

    public ReabrirCargaMesaHandler(ICargaMesaDocenteRepository cargas, IUnitOfWork unitOfWork, TimeProvider reloj)
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
            return Result.Error<int>("No hay ninguna carga para esta mesa.");
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
            ? Result.Warning(carga.Id, "La carga estaba efectivizada: las notas ya volcadas no se deshacen; al volver a efectivizar se pisarán con los nuevos valores.")
            : Result.Ok(carga.Id);
    }
}
