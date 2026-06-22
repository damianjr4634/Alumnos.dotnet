using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Common;
using FluentValidation;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Reemplaza el set completo de permisos de un usuario (sucesor de
/// PermisosPorUsuario.BtnGrabarClick sobre BARRA_SEGU). Verifica que el usuario
/// exista y delega en YYY_SEGU_GRABA, que borra y reinserta en una sola pasada.
/// </summary>
public sealed class AsignarPermisosUsuarioHandler
{
    private readonly IUsuarioRepository _usuarios;
    private readonly ISeguGrabaProcedure _seguGraba;
    private readonly IValidator<AsignarPermisosUsuarioCommand> _validator;

    public AsignarPermisosUsuarioHandler(
        IUsuarioRepository usuarios,
        ISeguGrabaProcedure seguGraba,
        IValidator<AsignarPermisosUsuarioCommand> validator)
    {
        _usuarios = usuarios;
        _seguGraba = seguGraba;
        _validator = validator;
    }

    public async Task<Result<int>> HandleAsync(AsignarPermisosUsuarioCommand command, CancellationToken ct)
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

        return await _seguGraba.GrabarAsync(command.CodigoUsuario, command.CodigosOpcion, ct).ConfigureAwait(false);
    }
}
