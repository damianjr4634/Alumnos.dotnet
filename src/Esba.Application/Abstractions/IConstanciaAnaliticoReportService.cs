using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF tabular del analítico del alumno: la "Constancia de Materias
/// Aprobadas" (sucesor de <c>BitBtn1Click</c> de constanciaalumnos2.pas). Separado de
/// <see cref="IConstanciaReportService"/> (constancia de texto CTT/Pase/Analítico)
/// porque es un reporte de tabla por cuatrimestre con maqueta propia. Usa QuestPDF
/// (migration_improvements.md §3.5).
/// </summary>
public interface IConstanciaAnaliticoReportService
{
    byte[] GenerarMateriasAprobadas(ConstanciaMateriasAprobadasModel model);
}
