using Esba.Application.DTOs.Asistencias;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de XXX_FALTAS_IMPRESI: planilla de reincorporaciones/libres por
/// carrera, cuatrimestre/año y umbrales (sucesor de la impresión de inasistencias).
/// </summary>
public interface IPlanillaInasistenciasProcedure
{
    /// <summary>
    /// <paramref name="cutuco"/> "0" lista todas las comisiones. Los umbrales
    /// <paramref name="reincorporacionPrimera"/>/<paramref name="reincorporacionSegunda"/>/<paramref name="libre"/>
    /// son las cantidades de inasistencia que disparan cada estado.
    /// </summary>
    Task<IReadOnlyList<PlanillaReincorporacionDto>> ListarAsync(
        string codigoCarrera,
        string cutuco,
        string cuatrimestreAnio,
        decimal reincorporacionPrimera,
        decimal reincorporacionSegunda,
        decimal libre,
        CancellationToken ct);
}
