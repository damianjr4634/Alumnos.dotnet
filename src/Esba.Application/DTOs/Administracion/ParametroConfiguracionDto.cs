namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Fila de la pantalla de configuración general (XXX_CONF). Parame y Descripcion
/// son de solo lectura en la UI; el usuario solo edita Valor.
/// </summary>
public sealed record ParametroConfiguracionDto
{
    public required string Parame { get; init; }

    public string? Descripcion { get; init; }

    public string? Valor { get; init; }
}
