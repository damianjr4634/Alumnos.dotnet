using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Examenes;

/// <summary>
/// Caso de uso de lectura que arma un acta por comisión (A/REGULAR, Reincorporación
/// o Exámenes) y la entrega como PDF o Excel. Sucesor de la lógica de los
/// ImprimirClick/ExportExcel de lstactasARegular.pas, lstactasreincorporacion.pas y
/// lstactasexamenes.pas, sin SQL en la UI ni globales (§2.1, §2.3).
/// </summary>
public sealed class GenerarActaComisionHandler
{
    private readonly IValidator<GenerarActaComisionCommand> _validator;
    private readonly IActasQuery _actas;
    private readonly IConstanciasQuery _constancias;
    private readonly IActaReportService _reporte;
    private readonly IActaExcelService _excel;

    public GenerarActaComisionHandler(
        IValidator<GenerarActaComisionCommand> validator,
        IActasQuery actas,
        IConstanciasQuery constancias,
        IActaReportService reporte,
        IActaExcelService excel)
    {
        _validator = validator;
        _actas = actas;
        _constancias = constancias;
        _reporte = reporte;
        _excel = excel;
    }

    public async Task<Result<byte[]>> GenerarPdfAsync(GenerarActaComisionCommand command, CancellationToken ct)
    {
        var modelo = await ConstruirModeloAsync(command, ct).ConfigureAwait(false);
        return modelo.IsSuccess && modelo.Value is not null
            ? Result.Ok(_reporte.GenerarActaComision(modelo.Value))
            : Result.Error<byte[]>(modelo.Message ?? "No se pudo generar el acta.");
    }

    public async Task<Result<byte[]>> GenerarExcelAsync(GenerarActaComisionCommand command, CancellationToken ct)
    {
        var modelo = await ConstruirModeloAsync(command, ct).ConfigureAwait(false);
        return modelo.IsSuccess && modelo.Value is not null
            ? Result.Ok(_excel.GenerarActaComision(modelo.Value))
            : Result.Error<byte[]>(modelo.Message ?? "No se pudo generar el acta.");
    }

    private async Task<Result<ActaComisionModel>> ConstruirModeloAsync(
        GenerarActaComisionCommand command, CancellationToken ct)
    {
        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<ActaComisionModel>(validacion.Errors[0].ErrorMessage);
        }

        var descriptor = ActaComisionDescriptor.Para(command.Tipo);

        var carrera = await _constancias.ObtenerDatosCarreraAsync(command.CodigoCarrera, ct).ConfigureAwait(false);

        var cabeceras = await _actas.ObtenerCabecerasComisionAsync(
            command.CodigoCarrera, command.CuatrimestreAnio, command.Cutuco, command.CodigoMateria,
            descriptor.Condiciones, descriptor.FiltrarCabeceraPorCondicion, ct).ConfigureAwait(false);

        if (cabeceras.Count == 0)
        {
            return Result.Error<ActaComisionModel>("No hay datos para mostrar.");
        }

        var alumnos = await _actas.ObtenerAlumnosComisionAsync(
            command.CodigoCarrera, command.CuatrimestreAnio, command.Cutuco, command.CodigoMateria,
            descriptor.Condiciones, ct).ConfigureAwait(false);

        var alumnosPorComision = alumnos
            .GroupBy(a => (a.Cutuco, Mat: (a.CodigoMateria ?? string.Empty).Trim()))
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ActaAlumnoDto>)g.ToList());

        var secciones = cabeceras.Select(cabecera =>
        {
            var clave = (cabecera.Cutuco, Mat: (cabecera.CodigoMateria ?? string.Empty).Trim());
            var lista = alumnosPorComision.TryGetValue(clave, out var encontrados)
                ? encontrados
                : (IReadOnlyList<ActaAlumnoDto>)[];
            return new ActaComisionSeccion { Cabecera = cabecera, Alumnos = lista };
        }).ToList();

        var modelo = new ActaComisionModel
        {
            Tipo = command.Tipo,
            Titulo = descriptor.Titulo,
            CarreraLarga = carrera?.Nombre ?? command.CodigoCarrera,
            CuatrimestreAnio = command.CuatrimestreAnio,
            MuestraCorrespondienteCuatrimestre = descriptor.MuestraCorrespondienteCuatrimestre,
            Secciones = secciones,
        };

        return Result.Ok(modelo);
    }
}
