namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Una materia aprobada por equivalencia para el Art. 1° de la resolución terciaria
/// (sucesor del SELECT del loop de materias de lst_impresion_equivalencia_terc.pas).
/// Los datos de origen (materia/carrera/instituto/docente) los grabó el alta (9.3b).
/// </summary>
public sealed record MateriaEquivalenciaTerciariaDto
{
    /// <summary>MATERIAS.DESCRIPCI: nombre de la materia equivalida.</summary>
    public string? Descripcion { get; init; }

    /// <summary>MATERIAS.CUATRIM: cuatrimestre del plan.</summary>
    public int Cuatrimestre { get; init; }

    /// <summary>DOCENTES.DOCENTE (vía FEQDOCE): docente evaluador de la equivalencia.</summary>
    public string? Docente { get; init; }

    /// <summary>ANALITIC.FEQMATE: materia cursada en origen.</summary>
    public string? MateriaOrigen { get; init; }

    /// <summary>ANALITIC.FEQCARRE: carrera cursada en origen.</summary>
    public string? CarreraOrigen { get; init; }

    /// <summary>ANALITIC.FEQINST: establecimiento de origen.</summary>
    public string? InstitutoOrigen { get; init; }

    /// <summary>ANALITIC.ACTINT formateada "número/AA" (cast a integer ⇒ sin ceros a la izquierda).</summary>
    public string? ActaInterna { get; init; }
}
