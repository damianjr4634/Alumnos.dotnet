using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Blanqueo de contraseña por un administrador. Fija una clave temporal hasheada
/// y deja CAMPASS='S' para forzar el cambio en el próximo login del usuario.
/// Reemplaza el blanqueo legacy (PASSWD='/' + CAMPASS='S'), incompatible con PBKDF2.
/// </summary>
public sealed class BlanquearPasswordHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IValidator<BlanquearPasswordCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public BlanquearPasswordHandler(
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        IValidator<BlanquearPasswordCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(BlanquearPasswordCommand command, CancellationToken ct)
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

        usuario.PasswordHash = _hasher.Hash(command.PasswordTemporal);
        usuario.DebeCambiarPassword = true;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(usuario.Codigo);
    }
}
