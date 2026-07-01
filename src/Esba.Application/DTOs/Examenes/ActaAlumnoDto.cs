namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Alumno listado en un acta de examen. Sucesor de las filas de SqlDatos en las
/// pantallas legacy de actas (COD_ALU + APELLIDO + NOM_APE; PERM_EXA solo en las
/// actas volantes de mesa, que lo traen de XXX_MESAS_ALUMNOS).
/// </summary>
public sealed record ActaAlumnoDto
{
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    /// <summary>Cutuco de la cursada, para agrupar por comisión.</summary>
    public short Cutuco { get; init; }

    /// <summary>Código de materia, para agrupar por comisión.</summary>
    public string? CodigoMateria { get; init; }

    /// <summary>Número de permiso de examen (PERM_EXA). Solo en el acta volante de mesa.</summary>
    public int? PermisoExamen { get; init; }
}
