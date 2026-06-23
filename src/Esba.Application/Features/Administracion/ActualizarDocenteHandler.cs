using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Modificación de docente (sucesor del UPDATE de FrmAltaModProfes). El código
/// identifica la fila y no cambia; la baja se aplica por su propio flujo.
/// </summary>
public sealed class ActualizarDocenteHandler
{
    private readonly IDocenteRepository _docentes;
    private readonly IValidator<ActualizarDocenteCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarDocenteHandler(
        IDocenteRepository docentes,
        IValidator<ActualizarDocenteCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _docentes = docentes;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(ActualizarDocenteCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var codigo = command.Codigo.Trim();

        var docente = await _docentes.ObtenerPorCodigoAsync(codigo, ct).ConfigureAwait(false);
        if (docente is null)
        {
            return Result.Error<string>("El docente no existe.");
        }

        DocenteMapping.Aplicar(docente, command);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(codigo);
    }
}
