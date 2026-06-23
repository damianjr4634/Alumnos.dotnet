namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Guarda los valores editados de la pantalla de configuración (XXX_CONF). Solo
/// se actualiza el VALOR de parámetros ya existentes (identificados por PARAME);
/// la pantalla no da de alta ni de baja parámetros. Sucesor del UPDATE parametrizado
/// de TablaConfiguraciones.pas.
/// </summary>
public sealed record ActualizarConfiguracionCommand
{
    public required IReadOnlyList<ValorParametro> Valores { get; init; }
}

/// <summary>Par (PARAME, VALOR) editado en la grilla de configuración.</summary>
public sealed record ValorParametro
{
    public required string Parame { get; init; }

    public string? Valor { get; init; }
}
