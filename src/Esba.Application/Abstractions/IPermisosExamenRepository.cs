using Esba.Application.DTOs.Examenes;

namespace Esba.Application.Abstractions;

/// <summary>
/// Acceso a PERMEXA por Dapper (INDICE lo genera un trigger; la clave de negocio
/// es COD_ALU+CARRE+MESA+COD_MAT). Sucesor de las queries de PermisoExamen.pas.
/// </summary>
public interface IPermisosExamenRepository
{
    Task<IReadOnlyList<PermisoExamenDto>> ListarPorAlumnoAsync(string codigoCarrera, string codigoAlumno, CancellationToken ct);

    /// <summary>true si el alumno ya tiene permiso para esa materia/mesa.</summary>
    Task<bool> ExisteAsync(string codigoCarrera, string codigoAlumno, int mesa, string codigoMateria, CancellationToken ct);

    Task InsertarAsync(CrearPermisoExamenCommand permiso, CancellationToken ct);

    /// <summary>Inserta varios permisos en una sola transacción (carga masiva). Devuelve la cantidad insertada.</summary>
    Task<int> InsertarVariosAsync(IReadOnlyList<CrearPermisoExamenCommand> permisos, CancellationToken ct);

    Task<int> EliminarAsync(string codigoCarrera, string codigoAlumno, string codigoMateria, CancellationToken ct);
}
