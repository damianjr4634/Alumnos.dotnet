using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de XXX_MATERIAS_FINALES: materias que el alumno puede rendir en final
/// (con la validación de correlatividad ya resuelta por el SP) y su mesa.
/// </summary>
public interface IMateriasFinalesProcedure
{
    Task<IReadOnlyList<MateriaFinalDto>> ListarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct);
}
