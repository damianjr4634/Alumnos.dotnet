using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas de apoyo para emitir constancias de alumno (sucesoras de las consultas
/// embebidas de constanciaalumnos2.pas sobre CARRERA).
/// </summary>
public interface IConstanciasQuery
{
    /// <summary>
    /// Datos de la carrera para el encabezado y las firmas de la constancia.
    /// null si la carrera no existe.
    /// </summary>
    Task<CarreraConstanciaDto?> ObtenerDatosCarreraAsync(string codigoCarrera, CancellationToken ct);

    /// <summary>
    /// Encabezado de la equivalencia bachiller (alumno, actuación interna, secundario
    /// de origen, plan y carrera). null si el alumno no registra equivalencias en la carrera.
    /// </summary>
    Task<EncabezadoEquivalenciaBachillerDto?> ObtenerEncabezadoEquivalenciaBachillerAsync(
        string codigoAlumno, string codigoCarrera, CancellationToken ct);

    /// <summary>
    /// Cursada vigente que respalda la Constancia de Alumno Regular: el alumno debe
    /// estar CURSANDO/RECURSANDO en el cuatrimestre <paramref name="cuatrimestreVigente"/>
    /// (formato CUA_ANIO, p.ej. "124"). null si no está cursando (no se puede emitir).
    /// </summary>
    Task<AlumnoRegularDto?> ObtenerAlumnoRegularAsync(
        string codigoAlumno, string codigoCarrera, string cuatrimestreVigente, CancellationToken ct);
}
