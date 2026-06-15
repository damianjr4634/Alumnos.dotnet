using Esba.Application.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Esba.Web.Components.Shared;

/// <summary>
/// Definición de una columna de <see cref="EsbaListView{T}"/>. Es la única fuente
/// de verdad: la misma columna alimenta la grilla y la exportación a Excel/PDF
/// (sucesor del armado dinámico de columnas de modulovariable.pas).
/// </summary>
public sealed class EsbaColumn<T>
{
    public required string Titulo { get; init; }

    /// <summary>Valor crudo de la celda; se usa para el render por defecto y para exportar.</summary>
    public required Func<T, object?> Valor { get; init; }

    /// <summary>Formato .NET opcional (ej. "dd/MM/yyyy", "N2").</summary>
    public string? Formato { get; init; }

    /// <summary>Render personalizado de la celda (chips, iconos). Opcional.</summary>
    public RenderFragment<T>? Celda { get; init; }

    /// <summary>
    /// Clave de orden server-side (campo reconocido por la query). null = la
    /// columna no es ordenable.
    /// </summary>
    public string? ClaveOrden { get; init; }

    public bool AlinearDerecha { get; init; }

    /// <summary>Se incluye en las exportaciones (las columnas de acciones no).</summary>
    public bool Exportable { get; init; } = true;

    /// <summary>Oculta la columna en pantallas chicas (d-none d-md-table-cell).</summary>
    public bool OcultarEnChico { get; init; }

    public Align Alineacion => AlinearDerecha ? Align.Right : Align.Left;

    public string? ClaseResponsive => OcultarEnChico ? "d-none d-md-table-cell" : null;

    /// <summary>Texto formateado de la celda (cuando no hay render personalizado).</summary>
    public string TextoCelda(T fila) => AColumnaExportable().Formatear(fila);

    /// <summary>Proyecta a la columna de exportación equivalente (mismo valor/formato).</summary>
    public ColumnaExportable<T> AColumnaExportable() => new()
    {
        Titulo = Titulo,
        Valor = Valor,
        Formato = Formato,
        AlinearDerecha = AlinearDerecha,
    };
}
