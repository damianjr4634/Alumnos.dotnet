using Esba.Application.DTOs.Asistencias;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF de las carpetas por comisión (sucesor del dibujo GDI sobre Gnostice
/// de lstplanasis.pas y de lstNotasyPractico.pas con plantilla WMF): una hoja por
/// comisión con la nómina y la grilla en blanco que el docente completa a mano.
/// La maqueta la decide <c>model.Tipo</c> (asistencia o trabajos prácticos).
/// </summary>
public interface ICarpetaComisionReportService
{
    byte[] GenerarCarpeta(CarpetaComisionModel model);
}
