using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>
/// Genera el PDF de las actas de examen (sucesor del dibujo GDI sobre Gnostice
/// eDocEngine de las pantallas legacy de actas). Hoja Oficio/Legal, fiel al papel
/// volante que el tribunal completa a mano.
/// </summary>
public interface IActaReportService
{
    byte[] GenerarActaComision(ActaComisionModel model);

    byte[] GenerarActaMesa(ActaMesaModel model);
}
