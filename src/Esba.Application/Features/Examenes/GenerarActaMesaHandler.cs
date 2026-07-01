using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Examenes;

/// <summary>
/// Caso de uso de lectura que arma el acta volante de una mesa de examen y la
/// entrega como PDF o Excel. Sucesor de ImprimirClick/BtnExcelClick de
/// lstactasMesas.pas; los candidatos salen de XXX_MESAS_ALUMNOS (con su PERM_EXA).
/// </summary>
public sealed class GenerarActaMesaHandler
{
    private readonly IValidator<GenerarActaMesaCommand> _validator;
    private readonly IActasQuery _actas;
    private readonly IConstanciasQuery _constancias;
    private readonly IActaReportService _reporte;
    private readonly IActaExcelService _excel;

    public GenerarActaMesaHandler(
        IValidator<GenerarActaMesaCommand> validator,
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

    public async Task<Result<byte[]>> GenerarPdfAsync(GenerarActaMesaCommand command, CancellationToken ct)
    {
        var modelo = await ConstruirModeloAsync(command, ct).ConfigureAwait(false);
        return modelo.IsSuccess && modelo.Value is not null
            ? Result.Ok(_reporte.GenerarActaMesa(modelo.Value))
            : Result.Error<byte[]>(modelo.Message ?? "No se pudo generar el acta.");
    }

    public async Task<Result<byte[]>> GenerarExcelAsync(GenerarActaMesaCommand command, CancellationToken ct)
    {
        var modelo = await ConstruirModeloAsync(command, ct).ConfigureAwait(false);
        return modelo.IsSuccess && modelo.Value is not null
            ? Result.Ok(_excel.GenerarActaMesa(modelo.Value))
            : Result.Error<byte[]>(modelo.Message ?? "No se pudo generar el acta.");
    }

    private async Task<Result<ActaMesaModel>> ConstruirModeloAsync(GenerarActaMesaCommand command, CancellationToken ct)
    {
        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<ActaMesaModel>(validacion.Errors[0].ErrorMessage);
        }

        var cabecera = await _actas.ObtenerCabeceraMesaAsync(command.Mesa, command.CodigoCarrera, ct).ConfigureAwait(false);
        if (cabecera is null)
        {
            return Result.Error<ActaMesaModel>("No hay datos para mostrar.");
        }

        var carrera = await _constancias.ObtenerDatosCarreraAsync(command.CodigoCarrera, ct).ConfigureAwait(false);

        var alumnos = await _actas.ObtenerAlumnosMesaAsync(
            command.Mesa, command.CodigoCarrera, command.TipoExamen, ct).ConfigureAwait(false);

        var modelo = new ActaMesaModel
        {
            Titulo = $"ACTA DE EXAMEN {command.TipoExamen.ToUpperInvariant()}",
            CarreraLarga = carrera?.Nombre ?? command.CodigoCarrera,
            Mesa = command.Mesa,
            TipoExamen = command.TipoExamen,
            Cabecera = cabecera,
            Alumnos = alumnos,
        };

        return Result.Ok(modelo);
    }
}
