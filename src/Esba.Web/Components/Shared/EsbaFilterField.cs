namespace Esba.Web.Components.Shared;

/// <summary>
/// Tipo de control de un filtro declarativo. Sucesor de los tipos del array
/// <c>Param</c> de parametros.pas: C→Texto, N→Numero, D→Fecha, L/K→Seleccion,
/// X→MultiSeleccion; Booleano cubre los flags 'S'/'N' del legacy.
/// </summary>
public enum EsbaFilterKind
{
    Texto,
    Numero,
    Fecha,
    Seleccion,
    MultiSeleccion,
    Booleano,
}

/// <summary>Opción de un combo de filtro (valor enviado a la query + etiqueta visible).</summary>
public sealed record EsbaFilterOption(string Valor, string Etiqueta);

/// <summary>
/// Definición declarativa de un filtro para <see cref="EsbaFilterPanel"/>.
/// Reemplaza el armado en runtime de controles de parametros.pas; los combos
/// cargan por servicio (<see cref="OpcionesAsync"/>), nunca por SQL embebido (§3.3).
/// </summary>
public sealed class EsbaFilterField
{
    /// <summary>Clave con la que la página lee el valor (ej. "Carrera").</summary>
    public required string Clave { get; init; }

    public required string Etiqueta { get; init; }

    public required EsbaFilterKind Tipo { get; init; }

    /// <summary>Si es obligatorio, el panel no deja buscar sin valor.</summary>
    public bool Obligatorio { get; init; }

    /// <summary>Opciones estáticas para Seleccion/MultiSeleccion.</summary>
    public IReadOnlyList<EsbaFilterOption>? Opciones { get; init; }

    /// <summary>Origen asíncrono de opciones (combo cargado por servicio).</summary>
    public Func<Task<IReadOnlyList<EsbaFilterOption>>>? OpcionesAsync { get; init; }

    /// <summary>Valor inicial (ej. carrera activa, cuatrimestre vigente — &amp;CAR_ACT/&amp;CUA_ACT del legacy).</summary>
    public object? ValorInicial { get; init; }

    /// <summary>Ancho en columnas del MudGrid en breakpoint md (1-12).</summary>
    public int AnchoMd { get; init; } = 3;
}
