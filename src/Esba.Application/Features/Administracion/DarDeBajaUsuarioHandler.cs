using Esba.Application.Abstractions;
using Esba.Domain.Common;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Baja lógica de un usuario (FECHA_BAJ = hoy). En el legacy el formulario
/// BajaUsuarios estaba inactivo: este comportamiento es nuevo (hito 10.1a).
/// Protege contra dejar el sistema inoperable: no se puede dar de baja uno mismo
/// ni al único supervisor activo. El usuario dado de baja no puede iniciar sesión
/// por el sistema nuevo (lo filtra IniciarSesionHandler).
/// </summary>
public sealed class DarDeBajaUsuarioHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public DarDeBajaUsuarioHandler(IUsuarioRepository usuarios, IUnitOfWork unitOfWork, TimeProvider timeProvider)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<int>> HandleAsync(int codigo, int codigoEjecutor, CancellationToken ct)
    {
        if (codigo == codigoEjecutor)
        {
            return Result.Error<int>("No podés darte de baja a vos mismo.");
        }

        var usuario = await _usuarios.ObtenerPorCodigoAsync(codigo, ct).ConfigureAwait(false);
        if (usuario is null)
        {
            return Result.Error<int>("El usuario no existe.");
        }

        if (usuario.EstaDeBaja)
        {
            return Result.Warning(codigo, "El usuario ya estaba dado de baja.");
        }

        if (usuario.EsSupervisor
            && await _usuarios.ContarSupervisoresActivosAsync(ct).ConfigureAwait(false) <= 1)
        {
            return Result.Error<int>("No se puede dar de baja al único supervisor activo del sistema.");
        }

        usuario.FechaBaja = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(codigo);
    }
}
