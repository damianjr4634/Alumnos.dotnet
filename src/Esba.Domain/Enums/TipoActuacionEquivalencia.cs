namespace Esba.Domain.Enums;

/// <summary>
/// Tipo de actuación con la que se aprueba una equivalencia (radio Interna/D.G.E.G.P.
/// de Equivalencia.pas). Determina si el número va a ACTINT o ACTDGE de ANALITIC.
/// </summary>
public enum TipoActuacionEquivalencia
{
    /// <summary>Actuación interna del instituto: número autogenerado (ACTINT), se confirma en TBLEQUIVA.</summary>
    Interna,

    /// <summary>Actuación de la D.G.E.G.P.: número externo provisto por el operador (ACTDGE).</summary>
    Dgegp,
}
