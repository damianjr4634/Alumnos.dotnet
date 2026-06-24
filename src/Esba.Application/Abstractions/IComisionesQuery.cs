using Esba.Application.Common;
using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de comisiones armadas (COMARM).</summary>
public interface IComisionesQuery
{
    /// <summary>
    /// Listado paginado/filtrado/ordenado para la pantalla "Listado de comisiones"
    /// (server-side, §3.2). Hace el join a MATERIAS y DOCENTES como el legacy.
    /// </summary>
    Task<PagedResult<ComisionListItemDto>> BuscarAsync(ComisionesFiltro filtro, CancellationToken ct);

    /// <summary>
    /// Comisión completa para precargar el formulario de edición, con el horario
    /// decodificado en marcas por día. null si no existe.
    /// </summary>
    Task<ComisionDetailDto?> ObtenerDetalleAsync(
        string codigoCarrera, short cutuco, string codigoMateria, string cuatrimestreAnio, CancellationToken ct);

    /// <summary>
    /// Alumnos CURSANDO/RECURSANDO de una comisión (CUTUCO) en un cuatrimestre, con su
    /// mail, para el envío de correo por comisión (sucesor del SELECT de enviocorreo.pas).
    /// Excluye alumnos de baja (BAJA='N').
    /// </summary>
    Task<IReadOnlyList<AlumnoComisionCorreoDto>> ListarAlumnosDeComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, CancellationToken ct);
}
