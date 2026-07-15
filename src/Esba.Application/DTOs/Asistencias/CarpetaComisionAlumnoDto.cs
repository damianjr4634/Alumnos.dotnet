namespace Esba.Application.DTOs.Asistencias;

/// <summary>
/// Alumno de una hoja de carpeta por comisión. Sucesor de las filas de SqlDatos en
/// lstplanasis.pas / lstNotasyPractico.pas (CURSADA ⨝ ALUMNOS con condición
/// CURSANDO/RECURSANDO).
/// </summary>
public sealed record CarpetaComisionAlumnoDto
{
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    /// <summary>CONDICION de la cursada: CURSANDO o RECURSANDO (sección aparte en la hoja).</summary>
    public string? Condicion { get; init; }

    /// <summary>Cutuco de la cursada, para agrupar por comisión.</summary>
    public short Cutuco { get; init; }

    /// <summary>Código de materia, para agrupar por comisión.</summary>
    public string? CodigoMateria { get; init; }
}
