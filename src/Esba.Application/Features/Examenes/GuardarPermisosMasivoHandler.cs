using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Domain.Common;

namespace Esba.Application.Features.Examenes;

/// <summary>
/// Carga masiva de permisos de examen (sucesor de CargadePermisosMasivo.pas):
/// inserta en una transacción la lista de permisos armada en la grilla. Todos
/// deben ser de la misma carrera (la pantalla no mezcla carreras).
/// </summary>
public sealed class GuardarPermisosMasivoHandler
{
    private readonly IPermisosExamenRepository _permisos;

    public GuardarPermisosMasivoHandler(IPermisosExamenRepository permisos)
    {
        _permisos = permisos;
    }

    public async Task<Result<int>> HandleAsync(IReadOnlyList<CrearPermisoExamenCommand> permisos, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(permisos);

        if (permisos.Count == 0)
        {
            return Result.Error<int>("No hay permisos para grabar.");
        }

        if (permisos.Select(p => p.CodigoCarrera).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
        {
            return Result.Error<int>("La carga masiva no puede mezclar carreras.");
        }

        var insertados = await _permisos.InsertarVariosAsync(permisos, ct).ConfigureAwait(false);
        return Result.Ok(insertados);
    }
}
