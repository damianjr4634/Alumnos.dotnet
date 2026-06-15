using Esba.Application.DTOs.Asistencias;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de XXX_FALTAS_FALTAS: faltas ya cargadas de un alumno en una comisión
/// (para precargar el calendario de carga).
/// </summary>
public interface IFaltasAlumnoProcedure
{
    Task<IReadOnlyList<FaltaAlumnoDto>> ListarAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoAlumno, string? codigoMateria, CancellationToken ct);
}
