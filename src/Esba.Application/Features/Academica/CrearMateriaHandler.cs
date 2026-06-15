using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Alta de materia (sucesor del INSERT de altamodifmaterias.BotonGrabarClick).
/// Valida, normaliza el código y rechaza duplicados sobre la PK (CODMATERI,
/// CODCARRE) antes de tocar la base. Devuelve el código normalizado.
/// </summary>
public sealed class CrearMateriaHandler
{
    private readonly IMateriaRepository _materias;
    private readonly IValidator<CrearMateriaCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CrearMateriaHandler(
        IMateriaRepository materias,
        IValidator<CrearMateriaCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _materias = materias;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(CrearMateriaCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var codigo = MateriaMapping.NormalizarCodigo(command.Codigo);

        var existente = await _materias.ObtenerAsync(codigo, command.CodigoCarrera, ct).ConfigureAwait(false);
        if (existente is not null)
        {
            return Result.Error<string>($"Ya existe una materia con el código {codigo} en la carrera.");
        }

        var materia = new Materia { Codigo = codigo, CodigoCarrera = command.CodigoCarrera };
        MateriaMapping.Aplicar(materia, command, usuario);

        _materias.Agregar(materia);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(codigo);
    }
}
