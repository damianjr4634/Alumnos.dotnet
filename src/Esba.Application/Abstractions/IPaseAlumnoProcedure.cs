using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de <c>XXX_IMPRIME_PASE</c>: valida si se puede emitir el pase del alumno.
/// </summary>
public interface IPaseAlumnoProcedure
{
    /// <summary>
    /// Semántica FERRCOD: 0 → Ok (el alumno adeuda materias, corresponde el pase),
    /// 2 → Error (aprobó todas, no corresponde un pase).
    /// </summary>
    Task<Result<bool>> VerificarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct);
}
