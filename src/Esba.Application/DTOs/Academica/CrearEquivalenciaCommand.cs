using Esba.Domain.Enums;

namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Alta de una equivalencia de materia (solapa "Equivalencia" de Equivalencia.pas):
/// inserta en ANALITIC con CONDICION='EQUIVALENCIA'. Las solapas pase-regular y
/// pase-final del form legacy se difieren al hito 14.
/// </summary>
public sealed record CrearEquivalenciaCommand
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    public required string CodigoMateria { get; init; }

    /// <summary>Interna (número autogenerado) o D.G.E.G.P. (número externo).</summary>
    public required TipoActuacionEquivalencia TipoActuacion { get; init; }

    /// <summary>Número de actuación D.G.E.G.P. (requerido cuando TipoActuacion=Dgegp; el interno lo asigna el sistema).</summary>
    public string? NumeroDgegp { get; init; }

    /// <summary>Institución de origen donde cursó la materia (INSTITUT / FEQINST).</summary>
    public string? InstitutoOrigen { get; init; }

    /// <summary>Característica de la institución de origen (CARAC).</summary>
    public string? CaracteristicaOrigen { get; init; }

    /// <summary>Colegio de origen (COLEGIO).</summary>
    public string? Colegio { get; init; }

    /// <summary>Plan de origen (PLAN).</summary>
    public string? Plan { get; init; }

    /// <summary>Soporte documental presentado (A_C); puede no informarse.</summary>
    public DocumentoEquivalencia? Documento { get; init; }

    /// <summary>Código de docente de la materia equivalida en origen (FEQDOCE).</summary>
    public string? DocenteOrigen { get; init; }

    /// <summary>Nombre de la materia cursada en origen (FEQMATE).</summary>
    public string? MateriaOrigen { get; init; }

    /// <summary>Carrera cursada en origen (FEQCARRE).</summary>
    public string? CarreraOrigen { get; init; }
}
