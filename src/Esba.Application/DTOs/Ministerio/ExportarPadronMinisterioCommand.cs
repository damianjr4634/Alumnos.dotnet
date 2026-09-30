namespace Esba.Application.DTOs.Ministerio;

/// <summary>
/// Filtros del export Excel del padrón de comisiones al Ministerio. Sucesor del
/// BitBtn1Click ("Excel") de ComisionesAlMinisterio.pas, que preguntaba si exportar
/// todas las comisiones "Juntas" en una hoja o "Separadas" en una hoja por comisión.
/// </summary>
public sealed record ExportarPadronMinisterioCommand
{
    public required string CodigoCarrera { get; init; }

    /// <summary>CUA_ANIO en formato "d/aa" o "daa" (ej. "1/24").</summary>
    public required string CuatrimestreAnio { get; init; }

    /// <summary>Comisión (CUTUCO). Opcional: vacío exporta todas las comisiones del cuatrimestre.</summary>
    public short? Cutuco { get; init; }

    /// <summary>true: una hoja por comisión; false: todas las comisiones en una sola hoja.</summary>
    public bool HojasSeparadas { get; init; }
}
