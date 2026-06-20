using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF de la equivalencia bachiller (sucesor de
/// lst_impresion_equivalencia_bac.pas): resolución interna, documento secundario a la
/// vista y listado de materias a dos columnas. Reporte de maqueta propia (separado de
/// las constancias de texto y del analítico tabular); usa QuestPDF (§3.5).
/// </summary>
public interface IEquivalenciaBachillerReportService
{
    byte[] GenerarEquivalenciaBachiller(EquivalenciaBachillerModel model);
}
