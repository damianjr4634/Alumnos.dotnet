using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de <c>XXX_CONSTANCIA_TERCIARIA</c>: detalle de materias del alumno con
/// su condición (alimenta el cálculo de "materias que adeuda" y, en 9.2, la grilla
/// del analítico tabular).
/// </summary>
public interface IConstanciaMateriasProcedure
{
    Task<IReadOnlyList<ConstanciaMateriaDto>> ListarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct);
}
