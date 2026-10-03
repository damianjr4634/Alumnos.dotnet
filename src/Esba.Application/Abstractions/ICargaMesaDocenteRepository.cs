using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

/// <summary>Escrituras EF sobre DOC_CARGA_MESA(_DET) (hito 19).</summary>
public interface ICargaMesaDocenteRepository
{
    /// <summary>La carga de una mesa con sus detalles, trackeada; null si todavía no existe.</summary>
    Task<CargaMesaDocente?> ObtenerPorMesaAsync(ClaveMesa mesa, CancellationToken ct);

    void Agregar(CargaMesaDocente carga);
}

/// <summary>Lecturas Dapper de la precarga de una mesa (hito 19).</summary>
public interface ICargaMesaDocenteQuery
{
    /// <summary>
    /// Mesa + cabecera de la carga (si existe) + alumnos con permiso (PERMEXA, no dados
    /// de baja) con su condición actual y lo precargado. Null si la mesa no existe.
    /// </summary>
    Task<CargaMesaDocenteDto?> ObtenerAsync(ClaveMesa mesa, CancellationToken ct);
}
