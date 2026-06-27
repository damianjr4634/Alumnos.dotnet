namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Filtro + paginación del listado de mesas de examen (sucesor del SELECT por
/// carrera de MesasExamen.pas, ahora server-side).
/// </summary>
public sealed record MesasFiltro
{
    public required string CodigoCarrera { get; init; }

    /// <summary>MESA: filtra por número de mesa (opcional; el operador la identifica por número).</summary>
    public int? Mesa { get; init; }

    /// <summary>COD_MAT: filtra por una materia (opcional).</summary>
    public string? CodigoMateria { get; init; }

    /// <summary>Texto libre sobre sigla/descripción de la materia.</summary>
    public string? Texto { get; init; }

    public string? OrdenarPor { get; init; }

    public bool OrdenDescendente { get; init; }

    public int Skip { get; init; }

    public int Take { get; init; } = 25;
}
