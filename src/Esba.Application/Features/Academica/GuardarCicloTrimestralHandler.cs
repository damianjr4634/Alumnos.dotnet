using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Alta/modificación de las fechas de los trimestres "uso 333" de un año
/// lectivo (TBL_TRIM). Sucesor de CargadeTrimestres.pas (modo TRIMESTRAL).
/// </summary>
public sealed class GuardarCicloTrimestralHandler
{
    private readonly ICicloLectivoRepository _ciclos;
    private readonly IValidator<GuardarCicloTrimestralCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public GuardarCicloTrimestralHandler(
        ICicloLectivoRepository ciclos,
        IValidator<GuardarCicloTrimestralCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _ciclos = ciclos;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(GuardarCicloTrimestralCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var existente = await _ciclos.ObtenerTrimestralAsync(command.Anio, ct).ConfigureAwait(false);
        if (command.EsNuevo && existente is not null)
        {
            return Result.Error<int>($"El año {command.Anio} ya está cargado.");
        }

        if (!command.EsNuevo && existente is null)
        {
            return Result.Error<int>($"El año {command.Anio} no existe.");
        }

        var ciclo = existente ?? new CicloTrimestral { Anio = command.Anio };
        ciclo.PrimerTrimestreDesde = command.PrimerTrimestreDesde!.Value;
        ciclo.PrimerTrimestreHasta = command.PrimerTrimestreHasta!.Value;
        ciclo.SegundoTrimestreDesde = command.SegundoTrimestreDesde!.Value;
        ciclo.SegundoTrimestreHasta = command.SegundoTrimestreHasta!.Value;
        ciclo.TercerTrimestreDesde = command.TercerTrimestreDesde!.Value;
        ciclo.TercerTrimestreHasta = command.TercerTrimestreHasta!.Value;

        if (existente is null)
        {
            _ciclos.Agregar(ciclo);
        }

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result.Ok(command.Anio);
    }
}
