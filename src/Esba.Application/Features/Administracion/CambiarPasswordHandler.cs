using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Cambio de contraseña por el propio usuario (sucesor de CambioPassword.GrabaClick).
/// Verifica la clave actual con el mismo esquema dual del login (PBKDF2 o cifrado
/// legacy), guarda la nueva hasheada y deja CAMPASS='N'.
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

        var actualValida = _hasher.CanVerify(usuario.PasswordHash)
            ? _hasher.Verify(usuario.PasswordHash, command.PasswordActual)
            : _cipherLegacy.Descifrar(usuario.PasswordHash) == command.PasswordActual;

        if (!actualValida)
        {
            return Result.Error<int>("La contraseña actual es incorrecta.");
        }

        usuario.PasswordHash = _hasher.Hash(command.PasswordNueva);
        usuario.DebeCambiarPassword = false;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(usuario.Codigo);
    }
}
