using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Examenes;

/// <summary>Alta de permiso de examen (sucesor de PermisoExamen.GrabaPermisoClick).</summary>
public sealed class CrearPermisoExamenHandler
{
    private readonly IPermisosExamenRepository _permisos;
    private readonly IValidator<CrearPermisoExamenCommand> _validator;

    public CrearPermisoExamenHandler(IPermisosExamenRepository permisos, IValidator<CrearPermisoExamenCommand> validator)
    {
        _permisos = permisos;
        _validator = validator;
    }

    public async Task<Result<string>> HandleAsync(CrearPermisoExamenCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<string>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var existe = await _permisos.ExisteAsync(
            command.CodigoCarrera, command.CodigoAlumno, command.Mesa, command.CodigoMateria, ct).ConfigureAwait(false);
        if (existe)
        {
            return Result.Error<string>("El alumno ya tiene un permiso para esa materia y mesa.");
        }

        await _permisos.InsertarAsync(command, ct).ConfigureAwait(false);
        return Result.Ok(command.CodigoMateria);
    }
}

/// <summary>Baja de permiso de examen (sucesor de PermisoExamen.eliminaPermisoClick).</summary>
public sealed class EliminarPermisoExamenHandler
{
    private readonly IPermisosExamenRepository _permisos;

    public EliminarPermisoExamenHandler(IPermisosExamenRepository permisos)
    {
        _permisos = permisos;
    }

    public async Task<Result<string>> HandleAsync(string codigoCarrera, string codigoAlumno, string codigoMateria, CancellationToken ct)
    {
        var borrados = await _permisos.EliminarAsync(codigoCarrera, codigoAlumno, codigoMateria, ct).ConfigureAwait(false);
        return borrados > 0
            ? Result.Ok(codigoMateria)
            : Result.Error<string>("El permiso no existe.");
    }
}
