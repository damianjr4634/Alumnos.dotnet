namespace Esba.Application.DTOs.Ministerio;

/// <summary>
/// Modelo de la nómina impresa de alumnos por comisión para el Ministerio (sucesor
/// del ImprimirClick de ComisionesAlMinisterio.pas): una hoja por comisión con los
/// cursantes y, debajo, los recursantes numerados aparte.
/// </summary>
public sealed record NominaMinisterioModel
{
    /// <summary>Nombre completo de la carrera ("Carrera: …").</summary>
    public required string CarreraLarga { get; init; }

    /// <summary>Año en curso ("Curso Lectivo: …").</summary>
    public required int CicloLectivo { get; init; }

    /// <summary>Fecha de emisión ("INSCRIPTOS AL: …").</summary>
    public required DateOnly FechaInscriptosAl { get; init; }

    /// <summary>Imprime sobre el papel membretado (checkbox "Con Membrete" del legacy).</summary>
    public required bool ConMembrete { get; init; }

    /// <summary>Agrega las columnas NACIONALIDAD y EDAD (checkbox "Edad y Nacionalidad" del legacy).</summary>
    public required bool ConEdadYNacionalidad { get; init; }

    /// <summary>Margen superior en centímetros (el legacy lo dejaba ajustar para papel preimpreso; default 4).</summary>
    public required decimal MargenSuperiorCm { get; init; }

    public required IReadOnlyList<NominaMinisterioSeccion> Secciones { get; init; }
}

/// <summary>Una comisión (CUTUCO) de la nómina, con cursantes y recursantes separados.</summary>
public sealed record NominaMinisterioSeccion
{
    public required short Cutuco { get; init; }

    public required IReadOnlyList<NominaMinisterioAlumnoDto> Cursando { get; init; }

    /// <summary>Recursantes: bloque "RECURSANTES" al pie de la comisión, numerados desde 1.</summary>
    public required IReadOnlyList<NominaMinisterioAlumnoDto> Recursantes { get; init; }
}

/// <summary>Alumno de la nómina impresa, ya formateado.</summary>
public sealed record NominaMinisterioAlumnoDto
{
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    /// <summary>"DNI 12.345.678" (columna DOCUMENTO).</summary>
    public required string Documento { get; init; }

    public string? Nacionalidad { get; init; }

    /// <summary>Edad en años cumplidos a la fecha de emisión.</summary>
    public int? Edad { get; init; }
}
