using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF de la citación a profesores (sucesor del dibujo GDI de
/// Imp_Mesas_citacion): una carta por docente sobre el membrete, en original y duplicado.
/// </summary>
public interface ICitacionDocentesReportService
{
    byte[] GenerarCitacion(CitacionDocentesModel model);
}

/// <summary>
/// Genera el PDF del parte diario de mesas (sucesor de Imp_Mesas_ParteDiario): una
/// hoja por fecha con las mesas agrupadas por carrera.
/// </summary>
public interface IParteDiarioMesasReportService
{
    byte[] GenerarParteDiario(ParteDiarioMesasModel model);
}
