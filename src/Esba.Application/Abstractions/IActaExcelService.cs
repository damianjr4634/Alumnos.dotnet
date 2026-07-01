using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el .xlsx de las actas de examen (sucesor de la exportación con plantilla
/// .xls vía automatización OLE de las pantallas legacy de actas). Implementado con
/// ClosedXML, sin Excel instalado (§3.5).
/// </summary>
public interface IActaExcelService
{
    byte[] GenerarActaComision(ActaComisionModel model);

    byte[] GenerarActaMesa(ActaMesaModel model);
}
