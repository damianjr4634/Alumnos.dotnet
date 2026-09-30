using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas de las impresiones de mesas de Impresiones.pas (citación a profesores y
/// parte diario). Todo SQL parametrizado (§1.3); sin globales.
/// </summary>
public interface IImpresionesMesasQuery
{
    /// <summary>
    /// Docentes que integran (titular, vocal 1 o vocal 2) alguna mesa del rango de
    /// fechas, acotados por rango de código de docente y carreras (ambos opcionales).
    /// </summary>
    Task<IReadOnlyList<DocenteCitadoDto>> ObtenerDocentesCitadosAsync(
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        string? codigoProfesorDesde,
        string? codigoProfesorHasta,
        IReadOnlyList<string> codigosCarrera,
        CancellationToken ct);

    /// <summary>
    /// Mesas del rango con una fila por docente convocado (mismos filtros que
    /// <see cref="ObtenerDocentesCitadosAsync"/>), ordenadas por docente, fecha y hora.
    /// </summary>
    Task<IReadOnlyList<MesaCitacionDto>> ObtenerMesasCitacionAsync(
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        string? codigoProfesorDesde,
        string? codigoProfesorHasta,
        IReadOnlyList<string> codigosCarrera,
        CancellationToken ct);

    /// <summary>Mesas del rango (y carrera opcional) con tribunal y cantidad de permisos, ordenadas por fecha, carrera y hora.</summary>
    Task<IReadOnlyList<ParteDiarioMesaDto>> ObtenerParteDiarioAsync(
        DateOnly fechaDesde, DateOnly fechaHasta, string? codigoCarrera, CancellationToken ct);

    /// <summary>Autoridades de la carrera (RECTOR, SECRETARIA, DIRESTU). null si no existe.</summary>
    Task<AutoridadesCarreraDto?> ObtenerAutoridadesAsync(string codigoCarrera, CancellationToken ct);
}
