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
}
