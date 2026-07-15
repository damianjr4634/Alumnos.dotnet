using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Asistencias;

/// <summary>
/// Caso de uso de lectura que arma una carpeta por comisión (planilla en blanco de
/// asistencia o de trabajos prácticos para la carpeta del docente) y la entrega como
/// PDF. Sucesor de los ImprimirClick de lstplanasis.pas ("Carpeta asistencia") y
/// lstNotasyPractico.pas ("Carpeta de trabajos practicos"), sin SQL en la UI ni
/// globales (§2.1, §2.3).
/// </summary>
public sealed class GenerarCarpetaComisionHandler
{
    private const string CondicionRecursando = "RECURSANDO";

    private readonly IValidator<GenerarCarpetaComisionCommand> _validator;
    private readonly ICarpetaComisionQuery _carpeta;
    private readonly IConstanciasQuery _constancias;
    private readonly ICarpetaComisionReportService _reporte;
    private readonly TimeProvider _clock;

    public GenerarCarpetaComisionHandler(
        IValidator<GenerarCarpetaComisionCommand> validator,
        ICarpetaComisionQuery carpeta,
        IConstanciasQuery constancias,
        ICarpetaComisionReportService reporte,
        TimeProvider clock)
    {
        _validator = validator;
        _carpeta = carpeta;
        _constancias = constancias;
        _reporte = reporte;
        _clock = clock;
    }

    public async Task<Result<byte[]>> GenerarPdfAsync(GenerarCarpetaComisionCommand command, CancellationToken ct)
    {
        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<byte[]>(validacion.Errors[0].ErrorMessage);
        }

        var carrera = await _constancias.ObtenerDatosCarreraAsync(command.CodigoCarrera, ct).ConfigureAwait(false);

        var cabeceras = await _carpeta.ObtenerComisionesAsync(
            command.CodigoCarrera, command.CuatrimestreAnio, command.Cutuco, command.CodigoMateria, ct)
            .ConfigureAwait(false);

        if (cabeceras.Count == 0)
        {
            return Result.Error<byte[]>("No hay datos para mostrar.");
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

        var modelo = new CarpetaComisionModel
        {
            Tipo = command.Tipo,
            CarreraLarga = carrera?.Nombre ?? command.CodigoCarrera,
            CuatrimestreAnio = command.CuatrimestreAnio,
            FechaEmision = DateOnly.FromDateTime(_clock.GetLocalNow().DateTime),
            Secciones = secciones,
        };

        return Result.Ok(_reporte.GenerarCarpeta(modelo));
    }

    private static bool EsRecursante(CarpetaComisionAlumnoDto alumno) =>
        string.Equals(alumno.Condicion?.Trim(), CondicionRecursando, StringComparison.OrdinalIgnoreCase);
}
