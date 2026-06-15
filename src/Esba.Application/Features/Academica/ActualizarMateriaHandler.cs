using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Modificación de materia (sucesor del UPDATE de
/// altamodifmaterias.BotonGrabarClick). El código y la carrera identifican la
/// fila y no cambian; la baja se aplica vía ESTADO='B' (no hay borrado físico).
/// </summary>
public sealed class ActualizarMateriaHandler
{
    private readonly IMateriaRepository _materias;
    private readonly IValidator<ActualizarMateriaCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarMateriaHandler(
        IMateriaRepository materias,
        IValidator<ActualizarMateriaCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _materias = materias;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(ActualizarMateriaCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var codigo = MateriaMapping.NormalizarCodigo(command.Codigo);

        var materia = await _materias.ObtenerAsync(codigo, command.CodigoCarrera, ct).ConfigureAwait(false);
        if (materia is null)
        {
            return Result.Error<string>("La materia no existe.");
        }

        MateriaMapping.Aplicar(materia, command, usuario);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(codigo);
    }
}
