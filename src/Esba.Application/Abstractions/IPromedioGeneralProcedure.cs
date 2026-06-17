namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de <c>XXX_PROMEDIO_GRAL</c>: promedio general del alumno en la carrera
/// (AVG de las notas de final del analítico, excluyendo las nulas/cero). Se muestra
/// en el encabezado del analítico tabular; el legacy no lo imprime en los reportes.
/// </summary>
public interface IPromedioGeneralProcedure
{
    /// <summary>Devuelve el promedio general; 0 si el alumno no tiene notas de final.</summary>
    Task<decimal> ObtenerAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct);
}
