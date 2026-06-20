namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Encabezado de la equivalencia bachiller (sucesor del <c>SELECT FIRST 1</c> embebido
/// en lst_impresion_equivalencia_bac.pas): cruza ANALITIC (equivalencia del alumno) con
/// ALUMNOS, TBLPLANES y CARRERA. null si el alumno no registra equivalencias en la carrera.
/// </summary>
public sealed record EncabezadoEquivalenciaBachillerDto
{
    /// <summary>APELLIDO + NOM_APE del alumno (ALUMNOS).</summary>
    public string? Alumno { get; init; }

    /// <summary>ANALITIC.ACTINT: número de actuación interna, sin formatear (el handler lo reformatea a "N°/AA").</summary>
    public string? ActividadInterna { get; init; }

    /// <summary>ANALITIC.A_C: 'C' título secundario en trámite (lleva nota AD-REFERENDUM); en otro caso, certificado analítico ya presentado.</summary>
    public string? DocumentoAC { get; init; }

    /// <summary>ANALITIC.INSTITUT: institución secundaria de origen (preferida sobre <see cref="Colegio"/> si no está vacía).</summary>
    public string? Instituto { get; init; }

    /// <summary>ANALITIC.COLEGIO: colegio secundario de origen.</summary>
    public string? Colegio { get; init; }

    /// <summary>COALESCE(TBLPLANES.FDESCRI, ANALITIC.PLAN): descripción del plan del secundario.</summary>
    public string? PlanDescripcion { get; init; }

    /// <summary>CARRERA.DESCARRE: nombre largo de la carrera (VCarreraLarga del legacy).</summary>
    public string? NombreCarrera { get; init; }

    /// <summary>CARRERA.TIPO: 'BAC'/'BAD' habilitan esta impresión; 'TER' no (el servidor revalida, §2.7).</summary>
    public string? TipoCarrera { get; init; }

    /// <summary>CARRERA.INSTITUT: instituto emisor (membrete). Distinto de <see cref="Instituto"/>, que es el secundario de origen.</summary>
    public string? InstitutoEmisor { get; init; }

    /// <summary>CARRERA.CARACT: característica del instituto emisor (membrete).</summary>
    public string? CaracteristicaEmisor { get; init; }
}
