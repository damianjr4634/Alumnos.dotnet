using Esba.Application.DTOs.Ministerio;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas del padrón de comisiones al Ministerio (sucesoras de los SELECT
/// concatenados de ComisionesAlMinisterio.pas). Todo SQL parametrizado (§1.3).
/// </summary>
public interface IPadronMinisterioQuery
{
    /// <summary>Datos de la carrera que codifican el padrón. null si no existe.</summary>
    Task<CarreraMinisterioDto?> ObtenerCarreraAsync(string codigoCarrera, CancellationToken ct);

    /// <summary>
    /// Comisiones (CUTUCO distintos) armadas en COMARM para la carrera y el cuatrimestre,
    /// que son las hojas de la nómina impresa. <paramref name="cuatrimestreAnio"/> se
    /// normaliza al formato de columna CHAR(3) "124" (sin barra).
    /// </summary>
    Task<IReadOnlyList<short>> ObtenerComisionesAsync(
        string codigoCarrera, string cuatrimestreAnio, short? cutuco, CancellationToken ct);

    /// <summary>
    /// Alumnos (CURSADA ⨝ ALUMNOS + tutor responsable) con condición CURSANDO o
    /// RECURSANDO y no dados de baja, en el orden del legacy (comisión, condición,
    /// apellido, nombre).
    /// </summary>
    Task<IReadOnlyList<PadronMinisterioAlumnoDto>> ObtenerAlumnosAsync(
        string codigoCarrera, string cuatrimestreAnio, short? cutuco, CancellationToken ct);
}
