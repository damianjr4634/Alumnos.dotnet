using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de XXX_VALIDO_MESA: pre-chequeo de duplicado antes de dar de alta una
/// mesa (sucesor de MesasExamen.MesaExit). Devuelve Error si la mesa ya existe.
/// </summary>
public interface IValidoMesaProcedure
{
    Task<Result<bool>> VerificarAsync(int numeroMesa, string codigoCarrera, CancellationToken ct);
}
