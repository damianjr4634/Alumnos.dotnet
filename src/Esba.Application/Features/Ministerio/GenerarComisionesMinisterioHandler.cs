using Esba.Application.Abstractions;
using Esba.Application.DTOs.Ministerio;
using Esba.Domain.Common;
using Esba.Domain.Ministerio;
using FluentValidation;

namespace Esba.Application.Features.Ministerio;

/// <summary>
/// Caso de uso de lectura "Comisiones al Ministerio" (sucesor de
/// ComisionesAlMinisterio.pas, menú Ministerio del legacy): la nómina impresa de
/// alumnos por comisión (ImprimirClick) y el padrón en Excel con el layout que pide el
/// Ministerio según el tipo de carrera (BitBtn1Click). Sin SQL en la UI ni globales
/// (§2.1, §2.3); el usuario que el legacy interpolaba en el mensaje de error llega por
/// claims si hiciera falta, acá el mensaje es neutro.
/// </summary>
public sealed class GenerarComisionesMinisterioHandler
{
    private const string SinDatos = "No hay datos para mostrar.";
    private const string CondicionRecursando = "RECURSANDO";

    private readonly IValidator<GenerarNominaMinisterioCommand> _validadorNomina;
    private readonly IValidator<ExportarPadronMinisterioCommand> _validadorPadron;
    private readonly IPadronMinisterioQuery _padron;
    private readonly INominaMinisterioReportService _reporte;
    private readonly IPadronMinisterioExcelService _excel;
    private readonly TimeProvider _clock;

    public GenerarComisionesMinisterioHandler(
        IValidator<GenerarNominaMinisterioCommand> validadorNomina,
        IValidator<ExportarPadronMinisterioCommand> validadorPadron,
        IPadronMinisterioQuery padron,
        INominaMinisterioReportService reporte,
        IPadronMinisterioExcelService excel,
        TimeProvider clock)
    {
        _validadorNomina = validadorNomina;
        _validadorPadron = validadorPadron;
        _padron = padron;
        _reporte = reporte;
        _excel = excel;
        _clock = clock;
    }

    /// <summary>Nómina impresa: una hoja por comisión de COMARM, con los alumnos de CURSADA.</summary>
    public async Task<Result<byte[]>> GenerarNominaPdfAsync(GenerarNominaMinisterioCommand command, CancellationToken ct)
    {
        var validacion = await _validadorNomina.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<byte[]>(validacion.Errors[0].ErrorMessage);
        }

        var carrera = await _padron.ObtenerCarreraAsync(command.CodigoCarrera, ct).ConfigureAwait(false);
        if (carrera is null)
        {
            return Result.Error<byte[]>("La carrera no existe.");
        }

        var comisiones = await _padron.ObtenerComisionesAsync(
            command.CodigoCarrera, command.CuatrimestreAnio, command.Cutuco, ct).ConfigureAwait(false);
        if (comisiones.Count == 0)
        {
            return Result.Error<byte[]>(SinDatos);
        }

        var alumnos = await _padron.ObtenerAlumnosAsync(
            command.CodigoCarrera, command.CuatrimestreAnio, command.Cutuco, ct).ConfigureAwait(false);

        var hoy = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime);
        var porComision = alumnos.ToLookup(a => a.Cutuco);

        var secciones = comisiones.Select(cutuco =>
        {
            var lista = porComision[cutuco].ToList();
            return new NominaMinisterioSeccion
            {
                Cutuco = cutuco,
                Cursando = lista.Where(a => !EsRecursante(a)).Select(a => PadronMinisterioMapper.AlumnoNomina(a, hoy)).ToList(),
                Recursantes = lista.Where(EsRecursante).Select(a => PadronMinisterioMapper.AlumnoNomina(a, hoy)).ToList(),
            };
        }).ToList();

        var modelo = new NominaMinisterioModel
        {
            CarreraLarga = carrera.Nombre ?? carrera.Codigo,
            CicloLectivo = hoy.Year,
            FechaInscriptosAl = hoy,
            ConMembrete = command.ConMembrete,
            ConEdadYNacionalidad = command.ConEdadYNacionalidad,
            MargenSuperiorCm = command.MargenSuperiorCm,
            Secciones = secciones,
        };

        return Result.Ok(_reporte.GenerarNomina(modelo));
    }

    /// <summary>Padrón Excel: filas de CURSADA codificadas con el layout terciario o secundario.</summary>
    public async Task<Result<byte[]>> ExportarExcelAsync(ExportarPadronMinisterioCommand command, CancellationToken ct)
    {
        var validacion = await _validadorPadron.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<byte[]>(validacion.Errors[0].ErrorMessage);
        }

        var carrera = await _padron.ObtenerCarreraAsync(command.CodigoCarrera, ct).ConfigureAwait(false);
        if (carrera is null)
        {
            return Result.Error<byte[]>("La carrera no existe.");
        }

        var alumnos = await _padron.ObtenerAlumnosAsync(
            command.CodigoCarrera, command.CuatrimestreAnio, command.Cutuco, ct).ConfigureAwait(false);
        if (alumnos.Count == 0)
        {
            return Result.Error<byte[]>(SinDatos);
        }

        var modelo = new PadronMinisterioModel
        {
            CodigoCarrera = carrera.Codigo,
            EsTerciaria = CodificacionMinisterio.EsTerciaria(carrera.Tipo),
            HojasSeparadas = command.HojasSeparadas,
            Filas = alumnos.Select(a => PadronMinisterioMapper.Fila(carrera, a)).ToList(),
        };

        return Result.Ok(_excel.GenerarPadron(modelo));
    }

    private static bool EsRecursante(PadronMinisterioAlumnoDto alumno) =>
        string.Equals(alumno.Condicion?.Trim(), CondicionRecursando, StringComparison.OrdinalIgnoreCase);
}
