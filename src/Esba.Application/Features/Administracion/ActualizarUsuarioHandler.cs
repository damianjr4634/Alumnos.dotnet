using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Modificación de datos del usuario (no toca la contraseña: tiene sus propios
/// flujos en 10.1c). Rechaza el cambio de nombre a uno ya usado por otro usuario.
/// No permite quitarle el rol de supervisor al último supervisor activo, para no
/// dejar el sistema sin administrador.
/// </summary>
public sealed class ActualizarUsuarioHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IValidator<ActualizarUsuarioCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarUsuarioHandler(
        IUsuarioRepository usuarios,
        IValidator<ActualizarUsuarioCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(ActualizarUsuarioCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var usuario = await _usuarios.ObtenerPorCodigoAsync(command.Codigo, ct).ConfigureAwait(false);
        if (usuario is null)
        {
            return Result.Error<int>("El usuario no existe.");
        }

        var nombre = command.NombreUsuario.Trim().ToUpperInvariant();
        if (await _usuarios.ExisteNombreAsync(nombre, command.Codigo, ct).ConfigureAwait(false))
        {
            return Result.Error<int>($"Ya existe otro usuario con el nombre '{nombre}'.");
        }

        // Si se le está quitando el rol de supervisor y era el último activo, se rechaza.
        if (usuario.EsSupervisor && !command.EsSupervisor && !usuario.EstaDeBaja
            && await _usuarios.ContarSupervisoresActivosAsync(ct).ConfigureAwait(false) <= 1)
        {
            return Result.Error<int>("No se puede quitar el rol de supervisor al único supervisor activo del sistema.");
        }

        usuario.NombreUsuario = nombre;
        usuario.Nombres = command.Nombres?.Trim();
        usuario.Apellido = command.Apellido?.Trim();
        usuario.Cargo = command.Cargo?.Trim();
        usuario.EsSupervisor = command.EsSupervisor;

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(usuario.Codigo);
    }
}
