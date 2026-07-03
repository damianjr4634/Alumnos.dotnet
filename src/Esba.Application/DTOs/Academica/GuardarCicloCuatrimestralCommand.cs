namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Alta o modificación de las fechas de los cuatrimestres de un año lectivo
/// (TBL_CUAT). Sucesor de CargadeTrimestres.pas en modo CUATRIMESTRAL, que
/// grababa con delete-all + reinsert sin validar; acá es un upsert por año.
/// </summary>
public sealed record GuardarCicloCuatrimestralCommand
{
    /// <summary>true = alta (el año no debe existir); false = edición (debe existir).</summary>
    public required bool EsNuevo { get; init; }

    public required int Anio { get; init; }

    public DateOnly? PrimerCuatrimestreDesde { get; init; }

    public DateOnly? PrimerCuatrimestreHasta { get; init; }

    public DateOnly? SegundoCuatrimestreDesde { get; init; }

    public DateOnly? SegundoCuatrimestreHasta { get; init; }
}
