using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Modifica turno/comisión, cuatrimestre y condición de una inscripción
/// (sucesor del UPDATE de GrabaMateriaClick en modo Modificacion). La materia
/// no se cambia (el legacy actualizaba la PK): se elimina y se inscribe de
/// nuevo. El pase RECURSANDO↔CURSANDO lo sincroniza el trigger con RECURSA.
/// </summary>
public sealed class ModificarInscripcionHandler
{
    private readonly ICursadaRepository _cursadas;
    private readonly IValidator<ModificarInscripcionCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarInscripcionHandler(
        ICursadaRepository cursadas,
        IValidator<ModificarInscripcionCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _cursadas = cursadas;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> HandleAsync(ModificarInscripcionCommand command, string usuario, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var cursada = await _cursadas.ObtenerAsync(command.CodigoCarrera, command.CodigoAlumno, command.CodigoMateria, ct).ConfigureAwait(false);
        if (cursada is null)
        {
            return Result.Error<string>("La inscripción no existe.");
        }

        cursada.Cutuco = InscribirEnMateriaHandler.ArmarCutuco(command.Turno, command.NumeroComision);
        cursada.CuatrimestreAnio = command.CuatrimestreAnio;
        cursada.Condicion = command.Condicion.Trim().ToUpperInvariant();
        cursada.Usuario = usuario;

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(command.CodigoMateria);
    }
}
