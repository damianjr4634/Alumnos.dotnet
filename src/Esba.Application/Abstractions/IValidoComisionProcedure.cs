using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de XXX_VALIDO_COMISION: pre-chequeo de duplicado antes de dar de alta
/// una comisión (sucesor de cargacomisiones.ComisionKeyPress). Devuelve Error si
/// la comisión ya existe para ese cuatrimestre.
/// </summary>
public interface IValidoComisionProcedure
{
    Task<Result<bool>> VerificarAsync(
        string cuatrimestreAnio, string codigoMateria, short cutuco, string codigoCarrera, CancellationToken ct);
}
