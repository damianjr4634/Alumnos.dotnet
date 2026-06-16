using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de <c>XXX_IMPRIME_CTT</c>: valida si se puede emitir el certificado de
/// estudios en trámite / analítico, y devuelve el cuatrimestre tope a considerar.
/// </summary>
public interface ICertificadoEnTramiteProcedure
{
    /// <summary>
    /// Devuelve un <see cref="Result{T}"/> cuyo valor es FCUATRI (cuatrimestre tope
    /// para "materias que adeuda"; 0 = sin tope). Semántica FERRCOD: 2 → Error
    /// (faltan materias), 1 → NeedsConfirmation (la carrera tiene título intermedio:
    /// se puede emitir hasta ese cuatrimestre), 0 → Ok.
    /// </summary>
    Task<Result<int>> VerificarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct);
}
