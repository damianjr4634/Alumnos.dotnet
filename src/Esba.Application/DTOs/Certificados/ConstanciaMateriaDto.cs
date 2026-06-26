namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Fila de materia del SP <c>XXX_CONSTANCIA_TERCIARIA</c> (que internamente delega
/// en <c>XXX_CONSTANCIA_ADM</c> para la carrera 'ADM'). En el hito 9.1 alimenta el
/// cálculo de "materias que adeuda"; en 9.2 será la grilla del analítico tabular,
/// por eso se proyectan ya las columnas de display.
/// </summary>
public sealed record ConstanciaMateriaDto
{
    public int Cuatrimestre { get; init; }

    public string? CodigoMateria { get; init; }

    public string? Sigla { get; init; }

    public string? Descripcion { get; init; }

    public string? Condicion { get; init; }

    public string? CuatrimestreAnio { get; init; }

    public decimal? Nota { get; init; }

    public DateOnly? Fecha { get; init; }

    public string? Instituto { get; init; }

    public string? Caracteristica { get; init; }

    /// <summary>'*' si es materia anual (ANUAL='S'), si no vacío.</summary>
    public string? Anual { get; init; }

    /// <summary>ACTINT: N° de acta interna cuando la materia se aprobó por equivalencia.</summary>
    public string? ActividadInterna { get; init; }

    /// <summary>ACTDEGP: N° de acta D.G.E.G.P. cuando la materia se aprobó por equivalencia.</summary>
    public string? ActividadDgegp { get; init; }

    /// <summary>EXIMDESC: descripción de la eximición (se imprime en la condición cuando CONDICION='EXIMIDO').</summary>
    public string? EximidoDescripcion { get; init; }

    /// <summary>HTMLCOLOR: color de fondo de la fila en formato CSS que calcula el SP (vacío en la rama ADM y en bases sin TBL_COLOR.HTMLCODE cargado).</summary>
    public string? HtmlColor { get; init; }

    /// <summary>HTMLFONTCOLOR: color de fuente de la fila en formato CSS que calcula el SP.</summary>
    public string? HtmlFontColor { get; init; }

    /// <summary>COLOR: color de fondo como TColor de la VCL (entero 0x00BBGGRR); fallback cuando HTMLCOLOR viene vacío.</summary>
    public int? ColorFondo { get; init; }

    /// <summary>FONTCOLOR: color de fuente como TColor de la VCL; fallback cuando HTMLFONTCOLOR viene vacío.</summary>
    public int? ColorFuente { get; init; }
}
