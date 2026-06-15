namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Filtro + paginación del listado de comisiones (sucesor del SELECT de
/// cargacomisiones.FormActivate: COMARM por carrera y cuatrimestre/año). La
/// carrera es obligatoria; el cuatrimestre/año por defecto es el vigente.
/// </summary>
public sealed record ComisionesFiltro
{
    public required string CodigoCarrera { get; init; }

    /// <summary>CUA_ANIO ("124" = 1/24); null = todos los cuatrimestres de la carrera.</summary>
    public string? CuatrimestreAnio { get; init; }

    /// <summary>COD_MAT: filtra por una materia (opcional).</summary>
    public string? CodigoMateria { get; init; }

    /// <summary>CODPROFES: filtra por un docente (opcional).</summary>
    public string? CodigoProfesor { get; init; }

    /// <summary>Texto libre sobre sigla/descripción de materia o nombre de docente.</summary>
    public string? Texto { get; init; }

    public string? OrdenarPor { get; init; }

    public bool OrdenDescendente { get; init; }

    public int Skip { get; init; }

    public int Take { get; init; } = 25;
}
