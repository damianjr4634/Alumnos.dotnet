namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Filtro + paginación del listado de materias de una carrera (sucesor del
/// listado de altamodifmaterias.pas, ahora server-side). La carrera es
/// obligatoria: el catálogo de materias siempre se mira dentro de una carrera.
/// </summary>
public sealed record MateriasFiltro
{
    /// <summary>Carrera a listar (obligatoria).</summary>
    public required string CodigoCarrera { get; init; }

    /// <summary>Texto libre que matchea descripción o sigla (CONTAINING).</summary>
    public string? Texto { get; init; }

    /// <summary>Cuatrimestre del plan (1, 2, …); null = todos.</summary>
    public short? Cuatrimestre { get; init; }

    /// <summary>true = solo anuales, false = solo cuatrimestrales, null = todas.</summary>
    public bool? SoloAnuales { get; init; }

    /// <summary>true = solo con promoción, false = solo sin promoción, null = todas.</summary>
    public bool? SoloConPromocion { get; init; }

    /// <summary>Campo de orden (whitelist en la query); null = orden por defecto.</summary>
    public string? OrdenarPor { get; init; }

    public bool OrdenDescendente { get; init; }

    public int Skip { get; init; }

    public int Take { get; init; } = 25;
}
