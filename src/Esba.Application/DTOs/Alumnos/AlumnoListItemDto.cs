namespace Esba.Application.DTOs.Alumnos;

/// <summary>
/// Fila de la grilla principal del padrón (FrmEsba.MtAlumnos). Columnas según
/// el SELECT del padrón legacy; los datos de ficha rápida (foto, observaciones
/// de XXX_OBSERV_PANTA) se cargan aparte, no en el listado.
/// </summary>
public sealed record AlumnoListItemDto
{
    public required string Codigo { get; init; }

    public required string CodigoCarrera { get; init; }

    public string? Matriz { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    public string? Mail { get; init; }

    public bool Baja { get; init; }

    /// <summary>DESCARRE de CARRERA.</summary>
    public string? NombreCarrera { get; init; }

    /// <summary>RESOLUCION de CARRERA.</summary>
    public string? Resolucion { get; init; }

    /// <summary>"DISTANCIA" o "PRESENCIAL" según CARRERA.DISTANCIA.</summary>
    public required string Modalidad { get; init; }
}
