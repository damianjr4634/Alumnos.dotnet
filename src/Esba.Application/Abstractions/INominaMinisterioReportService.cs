using Esba.Application.DTOs.Ministerio;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF de la nómina de alumnos por comisión para el Ministerio (sucesor del
/// dibujo GDI sobre Gnostice del ImprimirClick de ComisionesAlMinisterio.pas): una
/// hoja por comisión con cursantes y recursantes al pie.
/// </summary>
public interface INominaMinisterioReportService
{
    byte[] GenerarNomina(NominaMinisterioModel model);
}
