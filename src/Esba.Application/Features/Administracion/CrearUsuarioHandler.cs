using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Alta de usuario (sucesor del INSERT de AltaUsuario.GrabaClick). Valida,
/// normaliza el nombre de login, rechaza duplicados (insensible a mayúsculas) y
/// hashea la contraseña con PBKDF2 antes de tocar la base (nunca en claro,
/// §2.7). El usuario nace con CAMPASS='S' para forzar el cambio de la clave
/// inicial en su primer login. Devuelve el CODUSU generado por el trigger.
/// </summary>
public sealed class CrearUsuarioHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<CrearUsuarioCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CrearUsuarioHandler(
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        IValidator<CrearUsuarioCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(CrearUsuarioCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        // El login compara el nombre en mayúsculas (UPPER en ambos lados): se
        // persiste normalizado para que no convivan duplicados que difieren solo
        // en el case.
        var nombre = command.NombreUsuario.Trim().ToUpperInvariant();

        if (await _usuarios.ExisteNombreAsync(nombre, null, ct).ConfigureAwait(false))
        {
            return Result.Error<int>($"Ya existe un usuario con el nombre '{nombre}'.");
        }

        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            PasswordHash = _hasher.Hash(command.Password),
            Nombres = command.Nombres?.Trim(),
            Apellido = command.Apellido?.Trim(),
            Cargo = command.Cargo?.Trim(),
            EsSupervisor = command.EsSupervisor,
            DebeCambiarPassword = true,
            FechaBaja = null,
        };

        _usuarios.Agregar(usuario);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(usuario.Codigo);
    }
}
