using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Login con verificación dual (sucesor de sesion.pas), conviviendo con el
/// escritorio Delphi que sigue validando PASSWD (decisión 2026-07-06):
/// 1. Si NPASSWD tiene valor, el usuario ya entró por la web: verifica solo
///    ese hash PBKDF2.
/// 2. Si NPASSWD es NULL (usuario nuevo en la web), valida contra PASSWD —
///    cifrado legacy EncriptoCadena2 o, si una versión anterior lo pisó con
///    "$E1$", contra ese hash — y en el login exitoso puebla NPASSWD y deja en
///    PASSWD el cifrado legacy de la contraseña tipeada (para los pisados esto
///    les repara el acceso por escritorio; para el resto es reescribir lo mismo).
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
        if (usuario is null || usuario.EstaDeBaja)
        {
            // Mismo mensaje genérico que el legacy: no se revela si el usuario existe
            // ni si está dado de baja (baja lógica, hito 10.1a).
            return Result.Error<SesionIniciadaDto>(MensajeCredencialesInvalidas);
        }

        bool credencialesValidas;
        if (usuario.PasswordHashNuevo is not null)
        {
            credencialesValidas = _hasher.Verify(usuario.PasswordHashNuevo, command.Password);
        }
        else
        {
            // Primer login web del usuario: PASSWD trae el cifrado legacy
            // (sesion.pas: EncriptoCadena2(PASSWD, -1) == contraseña tipeada) o
            // el hash "$E1$" que una versión anterior de este handler escribió ahí.
            credencialesValidas = _hasher.CanVerify(usuario.PasswordLegacy)
                ? _hasher.Verify(usuario.PasswordLegacy, command.Password)
                : _cipherLegacy.Descifrar(usuario.PasswordLegacy) == command.Password;
            if (credencialesValidas)
            {
                usuario.PasswordHashNuevo = _hasher.Hash(command.Password);
                // Garantiza que PASSWD quede legible para el escritorio: repara
                // los pisados con "$E1$" y es un no-op para los demás.
                usuario.PasswordLegacy = _cipherLegacy.Cifrar(command.Password);
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
