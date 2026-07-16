using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Domain.Asistencias;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Asistencias;

/// <summary>
/// Caso de uso de lectura que arma una carpeta por comisión (planilla en blanco de
/// asistencia, de trabajos prácticos o de calificaciones para la carpeta del docente)
/// y la entrega como PDF o Excel. Sucesor de los ImprimirClick/BtnExcelClick de
/// lstplanasis.pas ("Carpeta asistencia") y lstNotasyPractico.pas ("Carpeta de
/// trabajos practicos" y "Planillas de profesores"), sin SQL en la UI ni globales
/// (§2.1, §2.3).
/// </summary>
public sealed class GenerarCarpetaComisionHandler
{
    private const string CondicionRecursando = "RECURSANDO";

    private readonly IValidator<GenerarCarpetaComisionCommand> _validator;
    private readonly ICarpetaComisionQuery _carpeta;
    private readonly IConstanciasQuery _constancias;
    private readonly ICarpetaComisionReportService _reporte;
    private readonly ICarpetaComisionExcelService _excel;
    private readonly TimeProvider _clock;

    public GenerarCarpetaComisionHandler(
        IValidator<GenerarCarpetaComisionCommand> validator,
        ICarpetaComisionQuery carpeta,
        IConstanciasQuery constancias,
        ICarpetaComisionReportService reporte,
        ICarpetaComisionExcelService excel,
        TimeProvider clock)
    {
        _validator = validator;
        _carpeta = carpeta;
        _constancias = constancias;
        _reporte = reporte;
        _excel = excel;
        _clock = clock;
    }

    public async Task<Result<byte[]>> GenerarPdfAsync(GenerarCarpetaComisionCommand command, CancellationToken ct)
    {
        var modelo = await ArmarModeloAsync(command, ct).ConfigureAwait(false);
        return modelo.IsSuccess && modelo.Value is not null
            ? Result.Ok(_reporte.GenerarCarpeta(modelo.Value))
            : Result.Error<byte[]>(modelo.Message ?? "No se pudo generar la carpeta.");
    }

    public async Task<Result<byte[]>> GenerarExcelAsync(GenerarCarpetaComisionCommand command, CancellationToken ct)
    {
        // El Excel legacy existía solo en lstNotasyPractico (TP y planilla de
        // profesores); el de asistencia volcaba la grilla de comisiones, cubierta
        // por el listado de comisiones del hito 6.
        if (command.Tipo == TipoCarpetaComision.Asistencia)
        {
            return Result.Error<byte[]>("La carpeta de asistencia no tiene exportación a Excel.");
        }

        var modelo = await ArmarModeloAsync(command, ct).ConfigureAwait(false);
        return modelo.IsSuccess && modelo.Value is not null
            ? Result.Ok(_excel.GenerarCarpeta(modelo.Value))
            : Result.Error<byte[]>(modelo.Message ?? "No se pudo generar la carpeta.");
    }

    private async Task<Result<CarpetaComisionModel>> ArmarModeloAsync(
        GenerarCarpetaComisionCommand command, CancellationToken ct)
    {
        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<CarpetaComisionModel>(validacion.Errors[0].ErrorMessage);
        }

        var carrera = await _constancias.ObtenerDatosCarreraAsync(command.CodigoCarrera, ct).ConfigureAwait(false);

        var cabeceras = await _carpeta.ObtenerComisionesAsync(
            command.CodigoCarrera, command.CuatrimestreAnio, command.Cutuco, command.CodigoMateria, ct)
            .ConfigureAwait(false);

        if (cabeceras.Count == 0)
        {
            return Result.Error<CarpetaComisionModel>("No hay datos para mostrar.");
        }

        var alumnos = await _carpeta.ObtenerAlumnosAsync(
            command.CodigoCarrera, command.CuatrimestreAnio, command.Cutuco, command.CodigoMateria, ct)
            .ConfigureAwait(false);

        var alumnosPorComision = alumnos
            .GroupBy(a => (a.Cutuco, Mat: (a.CodigoMateria ?? string.Empty).Trim()))
            .ToDictionary(g => g.Key, g => g.ToList());

        var secciones = cabeceras.Select(cabecera =>
        {
            var clave = (cabecera.Cutuco, Mat: (cabecera.CodigoMateria ?? string.Empty).Trim());
            var lista = alumnosPorComision.TryGetValue(clave, out var encontrados)
                ? encontrados
                : [];
            return new CarpetaComisionSeccion
            {
                Cabecera = cabecera,
                Cursando = lista.Where(a => !EsRecursante(a)).ToList(),
                Recursantes = lista.Where(EsRecursante).ToList(),
            };
        }).ToList();

        return Result.Ok(new CarpetaComisionModel
        {
            Tipo = command.Tipo,
            CarreraLarga = carrera?.Nombre ?? command.CodigoCarrera,
            CuatrimestreAnio = command.CuatrimestreAnio,
            FechaEmision = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime),
            Secciones = secciones,
        });
    }

    private static bool EsRecursante(CarpetaComisionAlumnoDto alumno) =>
        string.Equals(alumno.Condicion?.Trim(), CondicionRecursando, StringComparison.OrdinalIgnoreCase);
}
