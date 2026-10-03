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
/// Cambio de tipo y PASSWD (la columna del escritorio Delphi):
/// - Secretaría → docente/alumno: PASSWD pasa al sentinela de bloqueo de inmediato.
/// - Docente/alumno → secretaría: la contraseña real no se conoce (solo su hash),
///   así que PASSWD sigue bloqueado y se fuerza CAMPASS='S'; el próximo cambio de
///   contraseña del usuario la sincroniza y recién ahí puede entrar al escritorio.
///   Se devuelve Warning para que la UI lo avise.
/// </summary>
public sealed class ActualizarUsuarioHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IDocenteRepository _docentes;
    private readonly IAlumnoRepository _alumnos;
    private readonly IValidator<ActualizarUsuarioCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarUsuarioHandler(
        IUsuarioRepository usuarios,
        IDocenteRepository docentes,
        IAlumnoRepository alumnos,
        IValidator<ActualizarUsuarioCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _docentes = docentes;
        _alumnos = alumnos;
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

        var vinculo = await VinculoUsuario.ResolverAsync(command.Tipo, command.CodigoDocente,
            command.AlumnoCarrera, command.AlumnoCodigo, codigoUsuarioExcluido: command.Codigo,
            _usuarios, _docentes, _alumnos, ct).ConfigureAwait(false);
        if (vinculo.Error is not null)
        {
            return Result.Error<int>(vinculo.Error);
        }

        var usabaEscritorio = usuario.UsaEscritorio;

        usuario.NombreUsuario = nombre;
        usuario.Nombres = command.Nombres?.Trim();
        usuario.Apellido = command.Apellido?.Trim();
        usuario.Cargo = command.Cargo?.Trim();
        usuario.EsSupervisor = command.EsSupervisor;
        usuario.Tipo = command.Tipo;
        usuario.CodigoDocente = vinculo.CodigoDocente;
        usuario.AlumnoCarrera = vinculo.AlumnoCarrera;
        usuario.AlumnoCodigo = vinculo.AlumnoCodigo;

        string? aviso = null;
        if (usabaEscritorio && !usuario.UsaEscritorio)
        {
            usuario.PasswordLegacy = PasswordEscritorio.GenerarBloqueo();
        }
        else if (!usabaEscritorio && usuario.UsaEscritorio && PasswordEscritorio.EstaBloqueado(usuario.PasswordLegacy))
        {
            usuario.DebeCambiarPassword = true;
            aviso = "El usuario pasó a secretaría: deberá cambiar su contraseña en el próximo ingreso "
                + "para poder usar también el sistema de escritorio.";
        }

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return aviso is null ? Result.Ok(usuario.Codigo) : Result.Warning(usuario.Codigo, aviso);
    }
}
