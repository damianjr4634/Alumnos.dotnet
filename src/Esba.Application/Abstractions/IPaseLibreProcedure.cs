using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de XXX_FALTAS_PASLIBRE: pasa todas las materias CURSANDO del alumno a
/// LIBRES. El SP siempre pide confirmación (ERRCOD=1) y ya hace el UPDATE, así que
/// se ejecuta con el patrón de dos fases sin transacción de larga vida:
/// <paramref name="confirmar"/>=false ejecuta y hace rollback (devuelve el mensaje
/// de confirmación); true commitea.
/// </summary>
public interface IPaseLibreProcedure
{
    Task<Result<string>> EjecutarAsync(string codigoAlumno, string codigoCarrera, bool confirmar, CancellationToken ct);
}
