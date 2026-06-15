using Esba.Application.DTOs.Asistencias;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de XXX_FALTAS_COMISION: alumnos de una comisión con su acumulado de
/// faltas (sucesor del DsFaltas de CargaInasistenciasComisionNuevo).
/// </summary>
public interface IFaltasComisionProcedure
{
    Task<IReadOnlyList<AlumnoComisionFaltasDto>> ListarAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string? codigoMateria, CancellationToken ct);
}
