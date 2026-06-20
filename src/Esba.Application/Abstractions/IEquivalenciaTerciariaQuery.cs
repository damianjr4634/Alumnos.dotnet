using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas para la resolución de equivalencia terciaria (formato nuevo de
/// lst_impresion_equivalencia_terc.pas). Son SELECTs directos sobre ANALITIC/ALUMNOS/
/// MATERIAS/DOCENTES (el formato nuevo no usa <c>XXX_CONSTANCIA_TERCIARIA</c>).
/// </summary>
public interface IEquivalenciaTerciariaQuery
{
    /// <summary>Encabezado (alumno + lista de actas internas). null si el alumno no existe.</summary>
    Task<EncabezadoResolucionTerciariaDto?> ObtenerEncabezadoAsync(
        string codigoAlumno, string codigoCarrera, CancellationToken ct);

    /// <summary>
    /// Materias aprobadas por equivalencia en los cuatrimestres pedidos, ordenadas por
    /// cuatrimestre y orden del plan, para el Art. 1° de la resolución.
    /// </summary>
    Task<IReadOnlyList<MateriaEquivalenciaTerciariaDto>> ListarMateriasAsync(
        string codigoAlumno, string codigoCarrera, IReadOnlyCollection<int> cuatrimestres, CancellationToken ct);
}
