using Esba.Application.DTOs.Asistencias;

namespace Esba.Application.Abstractions;

/// <summary>Catálogo de tipos de inasistencia (TBL_FALTAS).</summary>
public interface ITipoFaltasQuery
{
    /// <summary>
    /// Tipos aplicables a la carrera: los globales (CARRE null) más los que la
    /// nombran (legacy: "CARRE IS NULL OR CARRE CONTAINING vcarrera").
    /// </summary>
    Task<IReadOnlyList<TipoFaltaDto>> ListarPorCarreraAsync(string codigoCarrera, CancellationToken ct);
}
