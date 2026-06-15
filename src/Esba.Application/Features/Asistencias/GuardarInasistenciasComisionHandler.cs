using System.Globalization;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Asistencias;

/// <summary>
/// Guarda las inasistencias de una comisión (sucesor de
/// CargaInasistenciasComisionNuevo.GrabamesaClick). Valida, deriva el año del
/// CUA_ANIO y reemplaza el conjunto de faltas de la comisión (delete+insert en
/// una transacción dentro del repositorio).
/// </summary>
public sealed class GuardarInasistenciasComisionHandler
{
    private readonly IInasistenciasRepository _inasistencias;
    private readonly IValidator<GuardarInasistenciasComisionCommand> _validator;

    public GuardarInasistenciasComisionHandler(
        IInasistenciasRepository inasistencias,
        IValidator<GuardarInasistenciasComisionCommand> validator)
    {
        _inasistencias = inasistencias;
        _validator = validator;
    }

    public async Task<Result<int>> HandleAsync(GuardarInasistenciasComisionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        // CUA_ANIO "124" → año 2024 (los dos últimos dígitos).
        var anio = 2000 + int.Parse(command.CuatrimestreAnio.Trim()[1..], CultureInfo.InvariantCulture);

        var insertados = await _inasistencias.ReemplazarFaltasComisionAsync(
            command.CodigoCarrera,
            command.Cutuco,
            string.IsNullOrWhiteSpace(command.CodigoMateria) ? null : command.CodigoMateria,
            anio,
            (short)command.CodigoUsuario,
            command.Faltas,
            ct).ConfigureAwait(false);

        return Result.Ok(insertados);
    }
}
