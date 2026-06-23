using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

/// <summary>
/// Acceso de escritura a XXX_CONF para el caso de uso de actualización. Devuelve
/// las entidades trackeadas para poder modificar su VALOR dentro de la transacción
/// del caso de uso (§1.3).
/// </summary>
public interface IConfiguracionRepository
{
    /// <summary>
    /// Trae los parámetros cuyos PARAME están en <paramref name="parames"/>,
    /// trackeados para edición. Los que no existan simplemente no se devuelven.
    /// </summary>
    Task<IReadOnlyList<ParametroConfiguracion>> ObtenerPorParamesAsync(
        IReadOnlyCollection<string> parames, CancellationToken ct);
}
