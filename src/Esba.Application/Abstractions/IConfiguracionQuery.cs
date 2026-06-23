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
}
