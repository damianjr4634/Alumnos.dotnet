using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Alta/modificación de las fechas de los cuatrimestres de un año lectivo
/// (TBL_CUAT). Sucesor de CargadeTrimestres.pas (modo CUATRIMESTRAL): en vez
/// del delete-all + reinsert del legacy, upsert del año dentro de la
/// transacción del caso de uso.
/// </summary>
public sealed class GuardarCicloCuatrimestralHandler
{
    private readonly ICicloLectivoRepository _ciclos;
    private readonly IValidator<GuardarCicloCuatrimestralCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public GuardarCicloCuatrimestralHandler(
        ICicloLectivoRepository ciclos,
        IValidator<GuardarCicloCuatrimestralCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _ciclos = ciclos;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(GuardarCicloCuatrimestralCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var existente = await _ciclos.ObtenerCuatrimestralAsync(command.Anio, ct).ConfigureAwait(false);
        if (command.EsNuevo && existente is not null)
        {
            return Result.Error<int>($"El año {command.Anio} ya está cargado.");
        }

        if (!command.EsNuevo && existente is null)
        {
            return Result.Error<int>($"El año {command.Anio} no existe.");
        }

        var ciclo = existente ?? new CicloCuatrimestral { Anio = command.Anio };
        ciclo.PrimerCuatrimestreDesde = command.PrimerCuatrimestreDesde!.Value;
        ciclo.PrimerCuatrimestreHasta = command.PrimerCuatrimestreHasta!.Value;
        ciclo.SegundoCuatrimestreDesde = command.SegundoCuatrimestreDesde!.Value;
        ciclo.SegundoCuatrimestreHasta = command.SegundoCuatrimestreHasta!.Value;

        if (existente is null)
        {
            _ciclos.Agregar(ciclo);
        }

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);
        return Result.Ok(command.Anio);
    }
}
