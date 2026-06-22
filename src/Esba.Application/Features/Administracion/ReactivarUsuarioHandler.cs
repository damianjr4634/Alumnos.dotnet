using Esba.Application.Abstractions;
using Esba.Domain.Common;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Reactiva un usuario dado de baja (FECHA_BAJ = NULL), volviéndolo a habilitar
/// para iniciar sesión. Inversa de <see cref="DarDeBajaUsuarioHandler"/> (hito 10.1a).
/// </summary>
public sealed class ReactivarUsuarioHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ReactivarUsuarioHandler(IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(int codigo, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorCodigoAsync(codigo, ct).ConfigureAwait(false);
        if (usuario is null)
        {
            return Result.Error<int>("El usuario no existe.");
        }

        if (!usuario.EstaDeBaja)
        {
            return Result.Warning(codigo, "El usuario ya estaba activo.");
        }

        usuario.FechaBaja = null;
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Ok(codigo);
    }
}
