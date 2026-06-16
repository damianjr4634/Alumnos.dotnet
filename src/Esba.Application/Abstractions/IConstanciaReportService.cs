using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF con formato propio de una constancia de alumno (sucesor del
/// dibujo GDI sobre TGmPreview de constanciaalumnos2.pas). La implementación usa
/// QuestPDF (migration_improvements.md §3.5). A diferencia de
/// <see cref="IPdfExportService"/> (volcado tabular de listados), esto es un
/// reporte maquetado.
/// </summary>
public interface IConstanciaReportService
{
    byte[] GenerarConstanciaAlumno(ConstanciaAlumnoModel model);
}
