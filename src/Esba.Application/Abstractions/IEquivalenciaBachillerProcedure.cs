using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de <c>XXX_IMPRESION_EQ_BAC</c>: arma el cuerpo de la equivalencia bachiller
/// (listado de materias de la carrera marcando cuáles tiene aprobadas por equivalencia
/// el alumno) en disposición a dos columnas. El SP usa una tabla temporal (GTT
/// <c>TMP_EQUI</c>, ON COMMIT DELETE ROWS) que se limpia sola al commitear.
/// </summary>
public interface IEquivalenciaBachillerProcedure
{
    Task<IReadOnlyList<LineaEquivalenciaBachillerDto>> ListarLineasAsync(
        string codigoAlumno, string codigoCarrera, CancellationToken ct);
}
