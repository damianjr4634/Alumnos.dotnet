using Esba.Application.Common;
using Esba.Application.DTOs.Alumnos;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas del padrón de alumnos (sucesor del SELECT que FrmEsba armaba por
/// concatenación y cargaba a MtAlumnos vía TFISQLThread).
/// </summary>
public interface IAlumnosQuery
{
    Task<PagedResult<AlumnoListItemDto>> BuscarPadronAsync(PadronAlumnosFiltro filtro, CancellationToken ct);

    /// <summary>Ficha completa del alumno (carga de FrmAltaAlumno en modo edición). Null si no existe.</summary>
    Task<AlumnoDetailDto?> ObtenerDetalleAsync(string codigoCarrera, string codigo, CancellationToken ct);
}
