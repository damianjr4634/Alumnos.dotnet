using Esba.Application.DTOs.Asistencias;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas de las carpetas por comisión (sucesoras de los SqlComi/SqlDatos de
/// lstplanasis.pas y lstNotasyPractico.pas, que compartían la misma nómina).
/// Todo SQL parametrizado (§1.3); sin concatenación ni globales.
/// </summary>
public interface ICarpetaComisionQuery
{
    /// <summary>
    /// Comisiones-materia de COMARM (con docente y flag titular/suplente) que cumplen
    /// el filtro. <paramref name="cuatrimestreAnio"/> se normaliza al formato de
    /// columna CHAR(3) "124" (sin barra).
    /// </summary>
    Task<IReadOnlyList<CarpetaComisionCabeceraDto>> ObtenerComisionesAsync(
        string codigoCarrera,
        string cuatrimestreAnio,
        short? cutuco,
        string? codigoMateria,
        CancellationToken ct);

    /// <summary>
    /// Alumnos (CURSADA ⨝ ALUMNOS) con condición CURSANDO o RECURSANDO para el mismo
    /// filtro. Se agrupan por comisión en el handler.
    /// </summary>
    Task<IReadOnlyList<CarpetaComisionAlumnoDto>> ObtenerAlumnosAsync(
        string codigoCarrera,
        string cuatrimestreAnio,
        short? cutuco,
        string? codigoMateria,
        CancellationToken ct);
}
