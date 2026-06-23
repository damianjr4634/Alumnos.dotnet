namespace Esba.Application.DTOs.Academica;

/// <summary>Filtro + paginación del listado de docentes (ABM de profesores), server-side (§3.2).</summary>
public sealed record DocentesFiltro
{
    /// <summary>Texto libre sobre código, nombre, documento o localidad.</summary>
    public string? Texto { get; init; }

    /// <summary>Si es false (default), solo trae docentes activos (FECHA_BAJ nula).</summary>
    public bool IncluirBajas { get; init; }

    public string? OrdenarPor { get; init; }

    public bool Descendente { get; init; }

    public int Skip { get; init; }

    public int Take { get; init; } = 25;
}
