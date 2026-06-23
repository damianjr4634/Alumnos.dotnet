using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Alta de docente (sucesor del INSERT de FrmAltaModProfes). Valida, normaliza el
/// código y rechaza duplicados sobre la PK (CODPROFES) antes de tocar la base.
/// </summary>
public sealed class CrearDocenteHandler
{
    private readonly IDocenteRepository _docentes;
    private readonly IValidator<CrearDocenteCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CrearDocenteHandler(
        IDocenteRepository docentes,
        IValidator<CrearDocenteCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _docentes = docentes;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(CrearDocenteCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var codigo = command.Codigo.Trim();

        if (await _docentes.ExisteAsync(codigo, ct).ConfigureAwait(false))
        {
            return Result.Error<string>($"Ya existe un docente con el código {codigo}.");
        }

        var docente = new Docente { Codigo = codigo };
        DocenteMapping.Aplicar(docente, command);

        _docentes.Agregar(docente);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(codigo);
    }
}
