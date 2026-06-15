namespace Esba.Web.Components.Shared;

/// <summary>
/// Valores actuales de un <see cref="EsbaFilterPanel"/>, con getters tipados por
/// clave para que la página arme su filtro sin castear a mano. Los valores se
/// guardan según el <see cref="EsbaFilterKind"/>: Texto/Seleccion → string?,
/// Numero → decimal?, Fecha → DateOnly?, MultiSeleccion → IReadOnlyList&lt;string&gt;,
/// Booleano → bool?.
/// </summary>
public sealed class EsbaFilterValores
{
    private readonly IReadOnlyDictionary<string, object?> _valores;

    public EsbaFilterValores(IReadOnlyDictionary<string, object?> valores) => _valores = valores;

    public string? Texto(string clave) =>
        _valores.TryGetValue(clave, out var v) && v is string s && !string.IsNullOrWhiteSpace(s) ? s.Trim() : null;

    public decimal? Numero(string clave) =>
        _valores.TryGetValue(clave, out var v) && v is decimal d ? d : null;

    public DateOnly? Fecha(string clave) =>
        _valores.TryGetValue(clave, out var v) && v is DateOnly f ? f : null;

    public bool? Booleano(string clave) =>
        _valores.TryGetValue(clave, out var v) && v is bool b ? b : null;

    public IReadOnlyList<string> MultiSeleccion(string clave) =>
        _valores.TryGetValue(clave, out var v) && v is IReadOnlyList<string> l ? l : [];

    /// <summary>Atajo: una selección como entero (cuatrimestre, etc.).</summary>
    public short? SeleccionShort(string clave)
    {
        var texto = Texto(clave);
        return texto is not null && short.TryParse(texto, out var n) ? n : null;
    }
}
