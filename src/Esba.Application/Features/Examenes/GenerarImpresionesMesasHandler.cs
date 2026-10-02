using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Common;
using Esba.Domain.Examenes;
using FluentValidation;

namespace Esba.Application.Features.Examenes;

/// <summary>
/// Caso de uso de lectura de las impresiones de mesas de Impresiones.pas (menú
/// Mesas del legacy): la citación a profesores (Imp_Mesas_citacion, "Impresion
/// citaciones") y el parte diario (Imp_Mesas_ParteDiario). Sin SQL en la UI ni globales
/// (§2.1, §2.3): las autoridades firmantes se leen de la carrera indicada en el comando,
/// no de FuncionesConfiguracion.
/// </summary>
public sealed class GenerarImpresionesMesasHandler
{
    private const string SinDatos = "No hay datos para mostrar.";

    private readonly IValidator<GenerarCitacionDocentesCommand> _validadorCitacion;
    private readonly IValidator<GenerarParteDiarioMesasCommand> _validadorParte;
    private readonly IImpresionesMesasQuery _query;
    private readonly ICitacionDocentesReportService _citacion;
    private readonly IParteDiarioMesasReportService _parteDiario;
    private readonly TimeProvider _clock;

    public GenerarImpresionesMesasHandler(
        IValidator<GenerarCitacionDocentesCommand> validadorCitacion,
        IValidator<GenerarParteDiarioMesasCommand> validadorParte,
        IImpresionesMesasQuery query,
        ICitacionDocentesReportService citacion,
        IParteDiarioMesasReportService parteDiario,
        TimeProvider clock)
    {
        _validadorCitacion = validadorCitacion;
        _validadorParte = validadorParte;
        _query = query;
        _citacion = citacion;
        _parteDiario = parteDiario;
        _clock = clock;
    }

    public async Task<Result<byte[]>> GenerarCitacionPdfAsync(GenerarCitacionDocentesCommand command, CancellationToken ct)
    {
        var validacion = await _validadorCitacion.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<byte[]>(validacion.Errors[0].ErrorMessage);
        }

        var autoridades = await _query.ObtenerAutoridadesAsync(command.CodigoCarreraFirma, ct).ConfigureAwait(false);
        if (autoridades is null)
        {
            return Result.Error<byte[]>("La carrera de firma no existe.");
        }

        var docentes = await _query.ObtenerDocentesCitadosAsync(
            command.FechaDesde, command.FechaHasta, command.CodigoProfesorDesde, command.CodigoProfesorHasta,
            command.CodigosCarrera, ct).ConfigureAwait(false);
        if (docentes.Count == 0)
        {
            return Result.Error<byte[]>(SinDatos);
        }

        var mesas = await _query.ObtenerMesasCitacionAsync(
            command.FechaDesde, command.FechaHasta, command.CodigoProfesorDesde, command.CodigoProfesorHasta,
            command.CodigosCarrera, ct).ConfigureAwait(false);
        var porDocente = mesas.ToLookup(m => m.CodigoProfesor.Trim(), StringComparer.Ordinal);

        var modelo = new CitacionDocentesModel
        {
            FechaEmision = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime),
            Firmante = command.Firmante,
            NombreFirmante = command.Firmante switch
            {
                FirmanteCitacion.Rector => autoridades.Rector,
                FirmanteCitacion.Secretaria => autoridades.Secretaria,
                _ => autoridades.DirectorEstudios,
            },
            Docentes = docentes.Select(d => new CitacionDocenteSeccion
            {
                CodigoProfesor = d.CodigoProfesor,
                Docente = d.Docente,
                Mesas = porDocente[d.CodigoProfesor.Trim()].ToList(),
            }).ToList(),
        };

        return Result.Ok(_citacion.GenerarCitacion(modelo));
    }

    public async Task<Result<byte[]>> GenerarParteDiarioPdfAsync(GenerarParteDiarioMesasCommand command, CancellationToken ct)
    {
        var validacion = await _validadorParte.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<byte[]>(validacion.Errors[0].ErrorMessage);
        }

        var mesas = await _query.ObtenerParteDiarioAsync(
            command.FechaDesde, command.FechaHasta,
            string.IsNullOrWhiteSpace(command.CodigoCarrera) ? null : command.CodigoCarrera.Trim(), ct)
            .ConfigureAwait(false);
        if (mesas.Count == 0)
        {
            return Result.Error<byte[]>(SinDatos);
        }

        // Los cortes por fecha y por carrera respetan el orden de la query (fecha,
        // carrera, hora), como los "fecha_aux"/"carre" del legacy.
        var dias = mesas
            .GroupBy(m => m.FechaExamen)
            .Select(dia => new ParteDiarioDia
            {
                Fecha = dia.Key,
                Carreras = dia
                    .GroupBy(m => m.CodigoCarrera.Trim(), StringComparer.Ordinal)
                    .Select(carrera => new ParteDiarioCarrera
                    {
                        CodigoCarrera = carrera.Key,
                        NombreCarrera = carrera.First().NombreCarrera,
                        Mesas = carrera.ToList(),
                    })
                    .ToList(),
            })
            .ToList();

        var modelo = new ParteDiarioMesasModel
        {
            CicloLectivo = _clock.GetLocalNow().Year,
            Dias = dias,
        };

        return Result.Ok(_parteDiario.GenerarParteDiario(modelo));
    }
}
