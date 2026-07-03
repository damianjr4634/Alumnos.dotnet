namespace Esba.Application.DTOs.Academica;

/// <summary>Fila de la grilla de ciclos trimestrales "uso 333" (TBL_TRIM, sucesor de CargadeTrimestres.pas).</summary>
public sealed record CicloTrimestralDto
{
    public required int Anio { get; init; }

    public required DateOnly PrimerTrimestreDesde { get; init; }

    public required DateOnly PrimerTrimestreHasta { get; init; }

    public required DateOnly SegundoTrimestreDesde { get; init; }

    public required DateOnly SegundoTrimestreHasta { get; init; }

    public required DateOnly TercerTrimestreDesde { get; init; }

    public required DateOnly TercerTrimestreHasta { get; init; }
}
