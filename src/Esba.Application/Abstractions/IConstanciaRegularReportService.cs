using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF de la Constancia de Alumno Regular (sucesor de CreatePDF de
/// constanciaalumnoregular.pas). Hoja A4 con membrete_con_direccion.jpg de fondo;
/// usa QuestPDF (§3.5).
/// </summary>
public interface IConstanciaRegularReportService
{
    byte[] GenerarConstanciaRegular(ConstanciaRegularModel model);
}
