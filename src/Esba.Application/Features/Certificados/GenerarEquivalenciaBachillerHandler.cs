using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Esba.Domain.Common;

namespace Esba.Application.Features.Certificados;

/// <summary>
/// Emite la equivalencia bachiller (sucesor de <c>ImprimirClick</c> de
/// lst_impresion_equivalencia_bac.pas). El servidor es autoritativo (§2.7): re-deriva el
/// encabezado desde ANALITIC y revalida que la carrera sea de bachillerato, en vez de
/// confiar en desde dónde se invocó. El reporte solo maqueta los textos ya resueltos.
/// </summary>
public sealed class GenerarEquivalenciaBachillerHandler
{
    private readonly IConstanciasQuery _carreras;
    private readonly IEquivalenciaBachillerProcedure _lineas;
    private readonly IEquivalenciaBachillerReportService _reporte;
    private readonly TimeProvider _clock;

    public GenerarEquivalenciaBachillerHandler(
        IConstanciasQuery carreras,
        IEquivalenciaBachillerProcedure lineas,
        IEquivalenciaBachillerReportService reporte,
        TimeProvider clock)
    {
        _carreras = carreras;
        _lineas = lineas;
        _reporte = reporte;
        _clock = clock;
    }

    public async Task<Result<byte[]>> GenerarPdfAsync(
        string codigoAlumno,
        string codigoCarrera,
        bool incluirMembrete,
        CancellationToken ct)
    {
        var encabezado = await _carreras
            .ObtenerEncabezadoEquivalenciaBachillerAsync(codigoAlumno, codigoCarrera, ct)
            .ConfigureAwait(false);
        if (encabezado is null)
        {
            return Result.Error<byte[]>("El alumno no registra equivalencias en esta carrera.");
        }

        if (!EquivalenciaBachillerFormatter.EsTipoBachiller(encabezado.TipoCarrera))
        {
            return Result.Error<byte[]>("La impresión de equivalencia bachiller solo aplica a carreras de bachillerato.");
        }

        var lineas = await _lineas.ListarLineasAsync(codigoAlumno, codigoCarrera, ct).ConfigureAwait(false);
        var hoy = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);

        var model = new EquivalenciaBachillerModel
        {
            NombreAlumno = encabezado.Alumno?.Trim() ?? string.Empty,
            CodigoAlumno = codigoAlumno.Trim(),
            ResolucionInterna = EquivalenciaBachillerFormatter.FormatearResolucionInterna(encabezado.ActividadInterna),
            NombreCarrera = encabezado.NombreCarrera,
            CicloLectivo = hoy.Year,
            Fecha = hoy,
            TextoVista = EquivalenciaBachillerFormatter.TextoVista(
                encabezado.DocumentoAC, encabezado.Instituto, encabezado.Colegio, encabezado.PlanDescripcion),
            MostrarNotaAdReferendum = EquivalenciaBachillerFormatter.EsTituloEnTramite(encabezado.DocumentoAC),
            Lineas = lineas,
            IncluirMembrete = incluirMembrete,
            Instituto = encabezado.InstitutoEmisor,
            Caracteristica = encabezado.CaracteristicaEmisor,
        };

        return Result.Ok(_reporte.GenerarEquivalenciaBachiller(model));
    }
}
