using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Esba.Domain.Common;

namespace Esba.Application.Features.Certificados;

/// <summary>
/// Emite la resolución de equivalencia terciaria (formato nuevo de
/// lst_impresion_equivalencia_terc.pas). El servidor es autoritativo (§2.7): revalida que
/// la carrera sea terciaria y que haya materias en los cuatrimestres pedidos. El reporte
/// solo maqueta los textos ya resueltos.
/// </summary>
public sealed class GenerarResolucionEquivalenciaTerciariaHandler
{
    private const string TipoTerciaria = "TER";

    private readonly IEquivalenciaTerciariaQuery _equivalencias;
    private readonly IConstanciasQuery _carreras;
    private readonly ICarrerasQuery _carrerasMeta;
    private readonly IResolucionEquivalenciaReportService _reporte;
    private readonly TimeProvider _clock;

    public GenerarResolucionEquivalenciaTerciariaHandler(
        IEquivalenciaTerciariaQuery equivalencias,
        IConstanciasQuery carreras,
        ICarrerasQuery carrerasMeta,
        IResolucionEquivalenciaReportService reporte,
        TimeProvider clock)
    {
        _equivalencias = equivalencias;
        _carreras = carreras;
        _carrerasMeta = carrerasMeta;
        _reporte = reporte;
        _clock = clock;
    }

    public async Task<Result<byte[]>> GenerarPdfAsync(
        string codigoAlumno,
        string codigoCarrera,
        string? cuatrimestres,
        bool incluirMembrete,
        CancellationToken ct)
    {
        var cuats = ResolucionEquivalenciaFormatter.ParsearCuatrimestres(cuatrimestres);
        if (cuats.Count == 0)
        {
            return Result.Error<byte[]>("Indique al menos un cuatrimestre a imprimir (por ejemplo: 2,3).");
        }

        var tipo = await _carrerasMeta.ObtenerTipoAsync(codigoCarrera, ct).ConfigureAwait(false);
        if (!string.Equals((tipo ?? string.Empty).Trim(), TipoTerciaria, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Error<byte[]>("La resolución de equivalencia terciaria solo aplica a carreras terciarias.");
        }

        var encabezado = await _equivalencias.ObtenerEncabezadoAsync(codigoAlumno, codigoCarrera, ct).ConfigureAwait(false);
        if (encabezado is null)
        {
            return Result.Error<byte[]>("El alumno no existe en la carrera.");
        }

        var materias = await _equivalencias.ListarMateriasAsync(codigoAlumno, codigoCarrera, cuats, ct).ConfigureAwait(false);
        if (materias.Count == 0)
        {
            return Result.Error<byte[]>("No hay materias aprobadas por equivalencia en los cuatrimestres indicados.");
        }

        var carrera = await _carreras.ObtenerDatosCarreraAsync(codigoCarrera, ct).ConfigureAwait(false);
        var hoy = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);

        var model = new ResolucionEquivalenciaTerciariaModel
        {
            Fecha = hoy,
            ActasInternas = encabezado.ActasInternas,
            TextoVisto = ResolucionEquivalenciaFormatter.TextoVisto(
                encabezado.NombreAlumno, encabezado.CodigoAlumno, encabezado.AnioActual, cuats, carrera?.Nombre),
            TextoConsiderando = ResolucionEquivalenciaFormatter.TextoConsiderando(carrera?.Rector),
            Materias = materias
                .Select(m => ResolucionEquivalenciaFormatter.ParrafoMateria(
                    m.Descripcion, m.Cuatrimestre, m.ActaInterna,
                    m.MateriaOrigen, m.CarreraOrigen, m.InstitutoOrigen, m.Docente))
                .ToArray(),
            Rector = carrera?.Rector,
            IncluirMembrete = incluirMembrete,
        };

        return Result.Ok(_reporte.GenerarResolucionTerciaria(model));
    }
}
