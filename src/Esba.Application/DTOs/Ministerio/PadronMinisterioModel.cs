namespace Esba.Application.DTOs.Ministerio;

/// <summary>
/// Modelo del export Excel del padrón de comisiones al Ministerio, listo para que el
/// servicio ClosedXML lo vuelque (sucesor de Exportar_Excel_DS / Exportar_Excel_DS_Hojas
/// de ComisionesAlMinisterio.pas).
/// </summary>
public sealed record PadronMinisterioModel
{
    public required string CodigoCarrera { get; init; }

    /// <summary>true: layout terciario (24 columnas); false: layout secundario (26 columnas, con adulto responsable).</summary>
    public required bool EsTerciaria { get; init; }

    /// <summary>true: una hoja por comisión ("Separadas" del legacy); false: todas en una hoja ("Juntas").</summary>
    public required bool HojasSeparadas { get; init; }

    /// <summary>Filas ya codificadas, en el orden del legacy (comisión, condición, apellido, nombre).</summary>
    public required IReadOnlyList<FilaPadronMinisterioDto> Filas { get; init; }
}
