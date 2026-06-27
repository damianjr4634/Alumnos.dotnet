using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>
/// Candidatos a cargar nota de final en una mesa. Sucesor del SELECT de
/// FinalesxMesayComision.BuscarMesaClick (usa el SP XXX_MESAS_ALUMNOS como
/// fuente, joineado con CURSADA para las notas/condición actuales).
/// </summary>
public interface ICargaFinalCandidatosQuery
{
    /// <param name="tipoExamen">
    /// Condición/llamado que filtra el SP: 'FINAL' para terciaria; 'LIBRES',
    /// 'PREVIOS', 'DICIEMBRE', 'MARZO' o 'P/EQUIVALEN' para bachiller.
    /// </param>
    Task<IReadOnlyList<CargaFinalAlumnoDto>> ObtenerAsync(
        int mesa, string codigoCarrera, string tipoExamen, CancellationToken ct);

    /// <summary>
    /// Candidatos de un solo alumno: todos sus permisos de examen (de cualquier
    /// mesa) ⨝ CURSADA. Sucesor del SELECT de NotasExamenFinal.FormCreate.
    /// </summary>
    Task<IReadOnlyList<CargaFinalAlumnoDto>> ObtenerPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct);
}
