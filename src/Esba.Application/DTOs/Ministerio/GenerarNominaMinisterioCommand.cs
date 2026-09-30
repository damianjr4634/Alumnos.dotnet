namespace Esba.Application.DTOs.Ministerio;

/// <summary>
/// Filtros y opciones de la nómina impresa de comisiones para el Ministerio. Sucesor
/// de los controles de ComisionesAlMinisterio.pas (comisión, cuatrimestre, "Con
/// Membrete", "Edad y Nacionalidad", margen superior); la carrera viene del contexto,
/// no del global VCarrera.
/// </summary>
public sealed record GenerarNominaMinisterioCommand
{
    public const decimal MargenSuperiorPorDefectoCm = 4m;

    public required string CodigoCarrera { get; init; }

    /// <summary>CUA_ANIO en formato "d/aa" o "daa" (lo que tipea el usuario, ej. "1/24").</summary>
    public required string CuatrimestreAnio { get; init; }

    /// <summary>Comisión (CUTUCO). Opcional: vacío imprime todas las comisiones del cuatrimestre.</summary>
    public short? Cutuco { get; init; }

    public bool ConMembrete { get; init; }

    public bool ConEdadYNacionalidad { get; init; }

    public decimal MargenSuperiorCm { get; init; } = MargenSuperiorPorDefectoCm;
}
