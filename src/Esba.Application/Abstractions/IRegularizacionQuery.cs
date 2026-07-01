using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas para la regularización de materias (sucesoras de la carga a "$$$CURSADA"
/// de RegularizacionDeMaterias_nuevo.pas y RegularizacionDeMateriasXComision_nuevo.pas).
/// SQL parametrizado; sin staging.
/// </summary>
public interface IRegularizacionQuery
{
    /// <summary>Todas las cursadas de un alumno en una carrera (variante por alumno).</summary>
    Task<IReadOnlyList<RegularizacionCursadaDto>> ObtenerPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct);

    /// <summary>
    /// Cursadas de una comisión-materia-cuatrimestre aún no regularizadas (variante por
    /// comisión): excluye CONDICION 'REGULAR' y alumnos dados de baja.
    /// </summary>
    Task<IReadOnlyList<RegularizacionCursadaDto>> ObtenerPorComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoMateria, CancellationToken ct);

    /// <summary>Todas las cursadas de bachillerato de un alumno en una carrera (variante por alumno).</summary>
    Task<IReadOnlyList<RegularizacionBachilleratoDto>> ObtenerBachilleratoPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct);

    /// <summary>
    /// Cursadas de bachillerato de una comisión-materia-cuatrimestre aún no regularizadas
    /// (variante por comisión): excluye CONDICION 'REGULAR' y alumnos dados de baja.
    /// </summary>
    Task<IReadOnlyList<RegularizacionBachilleratoDto>> ObtenerBachilleratoPorComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoMateria, CancellationToken ct);

    /// <summary>Todas las cursadas de secundario (333/650) de un alumno (variante por alumno).</summary>
    Task<IReadOnlyList<Regularizacion333Dto>> Obtener333PorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct);

    /// <summary>
    /// Cursadas de secundario (333/650) de una comisión-materia-cuatrimestre aún no
    /// regularizadas (variante por comisión): excluye CONDICION 'REGULAR' y alumnos de baja.
    /// </summary>
    Task<IReadOnlyList<Regularizacion333Dto>> Obtener333PorComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoMateria, CancellationToken ct);

    /// <summary>Todas las cursadas de CNA de un alumno (variante por alumno).</summary>
    Task<IReadOnlyList<RegularizacionCnaDto>> ObtenerCnaPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct);

    /// <summary>
    /// Cursadas de CNA de una comisión-materia-cuatrimestre aún no regularizadas (variante
    /// por comisión): excluye CONDICION 'REGULAR' y alumnos dados de baja.
    /// </summary>
    Task<IReadOnlyList<RegularizacionCnaDto>> ObtenerCnaPorComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoMateria, CancellationToken ct);
}
