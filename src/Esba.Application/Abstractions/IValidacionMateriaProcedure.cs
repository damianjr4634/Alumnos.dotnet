using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de <c>XXX_INSC_VALMAT</c>: valida si una materia se puede cargar para un
/// alumno (ya existe en cursada/analítico, correlatividades). El TIPO discrimina el
/// contexto: <c>'I'</c> inscripción de materias (chequea correlatividades),
/// <c>'A'</c> modificación de analítico/equivalencia (solo el duplicado). Devuelve
/// Error con el mensaje del SP cuando FERRCOD=2.
/// </summary>
public interface IValidacionMateriaProcedure
{
    Task<Result<bool>> ValidarAsync(string codigoAlumno, string codigoCarrera, string codigoMateria, char tipo, CancellationToken ct);
}
