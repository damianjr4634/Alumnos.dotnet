using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>Catálogo de tipos de mesa (MESA_TIPO).</summary>
public interface ITipoMesaQuery
{
    /// <summary>
    /// Tipos aplicables al "tipo" de la carrera (legacy: MESA_TIPO.CARRE CONTAINING
    /// CARRERA.TIPO), para el combo del ABM de mesas.
    /// </summary>
    Task<IReadOnlyList<TipoMesaDto>> ListarPorCarreraAsync(string codigoCarrera, CancellationToken ct);
}
