using Esba.Application.DTOs.Ministerio;

namespace Esba.Application.Abstractions;

/// <summary>
/// Exportación a Excel del padrón de comisiones al Ministerio (sucesora de
/// Exportar_Excel_DS / Exportar_Excel_DS_Hojas de ComisionesAlMinisterio.pas): un
/// libro con una hoja (todas las comisiones juntas) o una hoja por comisión, con el
/// layout terciario o secundario según la carrera.
/// </summary>
public interface IPadronMinisterioExcelService
{
    byte[] GenerarPadron(PadronMinisterioModel model);
}
