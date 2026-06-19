namespace Esba.Domain.Enums;

/// <summary>
/// Soporte documental de la equivalencia (checkboxes Constancia/Analítico de
/// Equivalencia.pas). Se guarda en ANALITIC.A_C ('C'/'A'); puede no informarse.
/// </summary>
public enum DocumentoEquivalencia
{
    /// <summary>A_C = 'C': se presentó constancia.</summary>
    Constancia,

    /// <summary>A_C = 'A': se presentó analítico.</summary>
    Analitico,
}
