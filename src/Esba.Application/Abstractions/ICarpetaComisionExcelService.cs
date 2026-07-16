using Esba.Application.DTOs.Asistencias;

namespace Esba.Application.Abstractions;

/// <summary>
/// Exportación a Excel de las carpetas por comisión (trabajos prácticos y planilla
/// de profesores). Sucesora del BtnExcel de lstNotasyPractico.pas, que automatizaba
/// OLE sobre la plantilla Planilla_de_notas.xls generando un archivo por
/// comisión/materia; acá igual: un .xlsx por comisión (en .zip si hay varias).
/// La carpeta de asistencia no exporta (el Excel legacy solo volcaba la grilla de
/// comisiones, cubierta por el listado de comisiones).
/// </summary>
public interface ICarpetaComisionExcelService
{
    CarpetaComisionExcelResultado GenerarCarpeta(CarpetaComisionModel model);
}
