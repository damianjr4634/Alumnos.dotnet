using Esba.Application.DTOs.Administracion;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lectura de los parámetros de configuración del sistema (XXX_CONF). La tabla es
/// chica y la pantalla legacy mostraba todo el padrón en una grilla, así que no se
/// pagina: se listan todos ordenados por PARAME.
/// </summary>
public interface IConfiguracionQuery
{
    Task<IReadOnlyList<ParametroConfiguracionDto>> ListarAsync(CancellationToken ct);

    /// <summary>
    /// Valor de un parámetro puntual (ej. Mail_EnvCopia). null si el parámetro no existe;
    /// puede ser cadena vacía si existe sin valor.
    /// </summary>
    Task<string?> ObtenerValorAsync(string parame, CancellationToken ct);
}
