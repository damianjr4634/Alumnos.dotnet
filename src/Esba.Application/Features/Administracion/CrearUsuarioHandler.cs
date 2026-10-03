using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Domain.Enums;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Alta de usuario (sucesor del INSERT de AltaUsuario.GrabaClick). Valida,
/// normaliza el nombre de login, rechaza duplicados (insensible a mayúsculas) y
/// guarda la clave inicial en NPASSWD hasheada con PBKDF2 (nunca en claro, §2.7).
/// PASSWD (la del escritorio Delphi) depende del tipo: secretaría recibe el
/// cifrado legacy para poder entrar también por el escritorio; docentes y alumnos
/// reciben el sentinela de <see cref="PasswordEscritorio"/> porque el Delphi no
/// mira USUARIOS.TIPO y no deben entrar por ahí. El vínculo (docente o alumno)
/// se verifica contra su tabla: sin FK físicas, esta es la barrera.
/// El usuario nace con CAMPASS='S' para forzar el cambio de la clave inicial en
/// su primer login. Devuelve el CODUSU generado por el trigger.
/// </summary>
public sealed class CrearUsuarioHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IDocenteRepository _docentes;
    private readonly IAlumnoRepository _alumnos;
    private readonly IPasswordHasher _hasher;
    private readonly ILegacyPasswordCipher _cipherLegacy;
    private readonly IValidator<CrearUsuarioCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public CrearUsuarioHandler(
        IUsuarioRepository usuarios,
        IDocenteRepository docentes,
        IAlumnoRepository alumnos,
        IPasswordHasher hasher,
        ILegacyPasswordCipher cipherLegacy,
        IValidator<CrearUsuarioCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _docentes = docentes;
        _alumnos = alumnos;
        _hasher = hasher;
        _cipherLegacy = cipherLegacy;
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

        var vinculo = await VinculoUsuario.ResolverAsync(command.Tipo, command.CodigoDocente,
            command.AlumnoCarrera, command.AlumnoCodigo, codigoUsuarioExcluido: null,
            _usuarios, _docentes, _alumnos, ct).ConfigureAwait(false);
        if (vinculo.Error is not null)
        {
            return Result.Error<int>(vinculo.Error);
        }

        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            PasswordHashNuevo = _hasher.Hash(command.Password),
            // Solo secretaría entra por el escritorio: para el resto, PASSWD queda bloqueado.
            PasswordLegacy = command.Tipo == TipoUsuario.Secretaria
                ? _cipherLegacy.Cifrar(command.Password)
                : PasswordEscritorio.GenerarBloqueo(),
            Nombres = command.Nombres?.Trim(),
            Apellido = command.Apellido?.Trim(),
            Cargo = command.Cargo?.Trim(),
            EsSupervisor = command.EsSupervisor,
            DebeCambiarPassword = true,
            FechaBaja = null,
            Tipo = command.Tipo,
            CodigoDocente = vinculo.CodigoDocente,
            AlumnoCarrera = vinculo.AlumnoCarrera,
            AlumnoCodigo = vinculo.AlumnoCodigo,
        };

        _usuarios.Agregar(usuario);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(usuario.Codigo);
    }
}
