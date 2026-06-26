namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Datos ya resueltos para maquetar la equivalencia bachiller (§2.1: el reporte no
/// calcula nada). Sucesor de los <c>TextOut</c> posicionados de
/// lst_impresion_equivalencia_bac.pas, reflowados a un documento QuestPDF.
/// </summary>
public sealed record EquivalenciaBachillerModel
{
    /// <summary>Apellido y nombre del alumno.</summary>
    public required string NombreAlumno { get; init; }

    /// <summary>Código del alumno (DNI/LM), mostrado junto al nombre.</summary>
    public required string CodigoAlumno { get; init; }

    /// <summary>Resolución interna ya formateada ("número/AA").</summary>
    public string? ResolucionInterna { get; init; }

    /// <summary>Nombre largo de la carrera (encabezado y pie).</summary>
    public string? NombreCarrera { get; init; }

    /// <summary>Año del ciclo lectivo de emisión.</summary>
    public required int CicloLectivo { get; init; }

    /// <summary>Fecha de emisión ("Buenos Aires, dd de MMMM de yyyy").</summary>
    public required DateOnly Fecha { get; init; }

    /// <summary>Frase "y teniendo a la vista …" con la institución de origen y el plan.</summary>
    public required string TextoVista { get; init; }

    /// <summary>true cuando el título está en trámite (A_C='C'): agrega la nota AD-REFERENDUM al pie.</summary>
    public bool MostrarNotaAdReferendum { get; init; }

    /// <summary>Cuerpo a dos columnas (materias de la carrera marcadas con equivalencia).</summary>
    public IReadOnlyList<LineaEquivalenciaBachillerDto> Lineas { get; init; } = [];

    /// <summary>INSTITUT de la carrera (membrete).</summary>
    public string? Instituto { get; init; }

    /// <summary>CARACT de la carrera (membrete).</summary>
    public string? Caracteristica { get; init; }
}
