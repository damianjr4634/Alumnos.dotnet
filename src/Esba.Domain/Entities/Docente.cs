namespace Esba.Domain.Entities;

/// <summary>
/// Tabla DOCENTES: profesores. Modelo mínimo para el join de comisiones (hito 6);
/// el ABM completo de profesores es el hito 10, que extenderá esta entidad.
/// PK: CODPROFES. Un docente está activo mientras <see cref="FechaBaja"/> es null
/// (el combo legacy de cargacomisiones filtra FECHA_BAJ IS NULL).
/// </summary>
public class Docente
{
    /// <summary>CODPROFES CHAR(3): código del docente (PK).</summary>
    public required string Codigo { get; set; }

    /// <summary>DOCENTE VARCHAR(80): apellido y nombre.</summary>
    public string? Nombre { get; set; }

    /// <summary>FECHA_ING DATE: fecha de ingreso.</summary>
    public DateOnly? FechaIngreso { get; set; }

    /// <summary>FECHA_BAJ DATE: fecha de baja; null = docente activo.</summary>
    public DateOnly? FechaBaja { get; set; }
}
