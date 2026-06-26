namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Datos resueltos para maquetar la resolución de equivalencia terciaria (§2.1: el
/// reporte no calcula). Sucesor del formato nuevo de lst_impresion_equivalencia_terc.pas,
/// consolidado en una sola resolución (VISTO → CONSIDERANDO → RESUELVE) con paginación
/// automática, en vez del doble render manual de páginas del legacy.
/// </summary>
public sealed record ResolucionEquivalenciaTerciariaModel
{
    /// <summary>Fecha de emisión ("Buenos Aires, dd de MMMM de yyyy").</summary>
    public required DateOnly Fecha { get; init; }

    /// <summary>Lista de actas internas ("200/19,201/19,…").</summary>
    public string? ActasInternas { get; init; }

    /// <summary>Párrafo del VISTO.</summary>
    public required string TextoVisto { get; init; }

    /// <summary>Párrafo del CONSIDERANDO (incluye el cierre del Rector/a).</summary>
    public required string TextoConsiderando { get; init; }

    /// <summary>Un párrafo por materia aprobada, en el orden del Art. 1°.</summary>
    public IReadOnlyList<string> Materias { get; init; } = [];

    /// <summary>Nombre del Rector/a (firma al pie).</summary>
    public string? Rector { get; init; }
}
