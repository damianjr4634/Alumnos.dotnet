using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas de docentes.</summary>
public interface IDocentesQuery
{
    /// <summary>
    /// Docentes activos (FECHA_BAJ IS NULL), ordenados por código — para el combo
    /// de docente del ABM de comisiones (sucesor de cargacomisiones.ComboDocente).
    /// </summary>
    Task<IReadOnlyList<DocenteListItemDto>> ListarActivosAsync(CancellationToken ct);
}
