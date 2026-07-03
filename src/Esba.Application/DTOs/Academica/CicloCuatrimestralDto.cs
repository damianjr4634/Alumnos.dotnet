namespace Esba.Application.DTOs.Academica;

/// <summary>Fila de la grilla de ciclos cuatrimestrales (TBL_CUAT, sucesor de CargadeTrimestres.pas).</summary>
public sealed record CicloCuatrimestralDto
{
    public required int Anio { get; init; }

    public required DateOnly PrimerCuatrimestreDesde { get; init; }

    public required DateOnly PrimerCuatrimestreHasta { get; init; }

    public required DateOnly SegundoCuatrimestreDesde { get; init; }

    public required DateOnly SegundoCuatrimestreHasta { get; init; }
}
