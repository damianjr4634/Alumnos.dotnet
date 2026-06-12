using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Login con verificación dual (sucesor de sesion.pas):
/// 1. Si PASSWD tiene el formato nuevo, verifica el hash PBKDF2.
/// 2. Si no, compara contra el cifrado legacy (EncriptoCadena2) y, si es
///    correcto, re-hashea en el momento (migration_improvements.md §2.7) — los
///    usuarios migran de esquema solos en su primer login.
/// En ambos casos regenera el UID de sesión única (regla de seciones.pas).
/// </summary>
public sealed class IniciarSesionHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly ILegacyPasswordCipher _cipherLegacy;
    private readonly IValidator<IniciarSesionCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    private const string MensajeCredencialesInvalidas = "Nombre de usuario o contraseña incorrectos.";

    public IniciarSesionHandler(
        IUsuarioRepository usuarios,
        IPasswordHasher hasher,
        ILegacyPasswordCipher cipherLegacy,
        IValidator<IniciarSesionCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _cipherLegacy = cipherLegacy;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SesionIniciadaDto>> HandleAsync(IniciarSesionCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validacion = await _validator.ValidateAsync(command, ct).ConfigureAwait(false);
        if (!validacion.IsValid)
        {
            return Result.Error<SesionIniciadaDto>(string.Join(" ", validacion.Errors.Select(e => e.ErrorMessage)));
        }

        var usuario = await _usuarios.ObtenerPorNombreConPermisosAsync(command.NombreUsuario.Trim(), ct).ConfigureAwait(false);
        if (usuario is null)
        {
            // Mismo mensaje genérico que el legacy: no se revela si el usuario existe.
            return Result.Error<SesionIniciadaDto>(MensajeCredencialesInvalidas);
        }

        bool credencialesValidas;
        if (_hasher.CanVerify(usuario.PasswordHash))
        {
            credencialesValidas = _hasher.Verify(usuario.PasswordHash, command.Password);
        }
        else
        {
            // Legacy (sesion.pas): EncriptoCadena2(PASSWD, -1) == contraseña tipeada.
            credencialesValidas = _cipherLegacy.Descifrar(usuario.PasswordHash) == command.Password;
            if (credencialesValidas)
            {
                usuario.PasswordHash = _hasher.Hash(command.Password);
            }
        }

        if (!credencialesValidas)
        {
            return Result.Error<SesionIniciadaDto>(MensajeCredencialesInvalidas);
        }

        usuario.SesionUid = Guid.NewGuid().ToString("N");
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(new SesionIniciadaDto
        {
            CodigoUsuario = usuario.Codigo,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = string.Join(", ", new[] { usuario.Apellido, usuario.Nombres }.Where(p => !string.IsNullOrWhiteSpace(p))),
            EsSupervisor = usuario.EsSupervisor,
            DebeCambiarPassword = usuario.DebeCambiarPassword,
            SesionUid = usuario.SesionUid,
            Permisos = usuario.Permisos.Select(p => p.CodigoOpcion).ToList(),
        });
    }
}
