namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Alta o modificación de las fechas de los trimestres "uso 333" de un año
/// lectivo (TBL_TRIM). Sucesor de CargadeTrimestres.pas en modo TRIMESTRAL.
/// </summary>
public sealed record GuardarCicloTrimestralCommand
{
    /// <summary>true = alta (el año no debe existir); false = edición (debe existir).</summary>
    public required bool EsNuevo { get; init; }

    public required int Anio { get; init; }

    public DateOnly? PrimerTrimestreDesde { get; init; }

    public DateOnly? PrimerTrimestreHasta { get; init; }

    public DateOnly? SegundoTrimestreDesde { get; init; }

    public DateOnly? SegundoTrimestreHasta { get; init; }

    public DateOnly? TercerTrimestreDesde { get; init; }

    public DateOnly? TercerTrimestreHasta { get; init; }
}
