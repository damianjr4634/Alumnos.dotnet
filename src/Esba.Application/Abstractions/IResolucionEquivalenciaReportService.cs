using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF de la resolución de equivalencia terciaria (formato nuevo de
/// lst_impresion_equivalencia_terc.pas): VISTO/CONSIDERANDO/RESUELVE sobre papel
/// membretado. Usa QuestPDF (§3.5).
/// </summary>
public interface IResolucionEquivalenciaReportService
{
    byte[] GenerarResolucionTerciaria(ResolucionEquivalenciaTerciariaModel model);
}
