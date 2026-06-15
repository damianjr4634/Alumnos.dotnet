namespace Esba.Web.Components.Shared;

/// <summary>
/// Pedido de una página de datos a la query server-side detrás de un
/// <see cref="EsbaListView{T}"/>. La página traduce esto al filtro concreto de
/// su query (paginación y orden resueltos en Firebird, §3.2).
/// </summary>
public sealed record EsbaListRequest
{
    public int Skip { get; init; }

    public int Take { get; init; } = 25;

    /// <summary>Clave de orden (la <see cref="EsbaColumn{T}.ClaveOrden"/> elegida); null = orden por defecto.</summary>
    public string? OrdenarPor { get; init; }

    public bool Descendente { get; init; }
}
