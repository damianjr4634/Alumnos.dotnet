using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Cambio de contraseña por el propio usuario (sucesor de CambioPassword.GrabaClick).
/// Verifica la clave actual con el mismo esquema dual del login (NPASSWD si existe;
/// si no, PASSWD legacy o pisado con "$E1$"), guarda la nueva en NPASSWD (PBKDF2) y
/// en PASSWD (cifrado legacy, para que el escritorio Delphi la acepte) y deja CAMPASS='N'.
/// </summary>
public sealed class CambiarPasswordHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly ILegacyPasswordCipher _cipherLegacy;
    private readonly IValidator<CambiarPasswordCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CambiarPasswordHandler(
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        ILegacyPasswordCipher cipherLegacy,
        IValidator<CambiarPasswordCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _cipherLegacy = cipherLegacy;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(CambiarPasswordCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<int>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var usuario = await _usuarios.ObtenerPorCodigoAsync(command.CodigoUsuario, ct).ConfigureAwait(false);
        if (usuario is null)
        {
            return Result.Error<int>("El usuario no existe.");
        }

        bool actualValida;
        if (usuario.PasswordHashNuevo is not null)
        {
            actualValida = _hasher.Verify(usuario.PasswordHashNuevo, command.PasswordActual);
        }
        else
        {
            actualValida = _hasher.CanVerify(usuario.PasswordLegacy)
                ? _hasher.Verify(usuario.PasswordLegacy, command.PasswordActual)
                : _cipherLegacy.Descifrar(usuario.PasswordLegacy) == command.PasswordActual;
        }

        if (!actualValida)
        {
            return Result.Error<int>("La contraseña actual es incorrecta.");
        }

        usuario.PasswordHashNuevo = _hasher.Hash(command.PasswordNueva);
        usuario.PasswordLegacy = _cipherLegacy.Cifrar(command.PasswordNueva);
        usuario.DebeCambiarPassword = false;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(usuario.Codigo);
    }
}
