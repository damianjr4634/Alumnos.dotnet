using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Wrapper de YYY_SEGU_GRABA: reemplaza el set completo de permisos de un usuario
/// (borra-todo-y-reinserta sobre BARRA_SEGU). Recibe los códigos habilitados.
/// </summary>
public interface ISeguGrabaProcedure
{
    Task<Result<int>> GrabarAsync(int codigoUsuario, IReadOnlyList<string> codigosOpcion, CancellationToken ct);
}
