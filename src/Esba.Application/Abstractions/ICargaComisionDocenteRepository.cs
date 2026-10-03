using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

/// <summary>Escrituras EF sobre DOC_CARGA_COMISION(_DET) (hito 19).</summary>
public interface ICargaComisionDocenteRepository
{
    /// <summary>La carga de una comisión con sus detalles, trackeada; null si todavía no existe.</summary>
    Task<CargaComisionDocente?> ObtenerPorComisionAsync(ClaveComision comision, CancellationToken ct);

    void Agregar(CargaComisionDocente carga);
}

/// <summary>Lecturas Dapper de la precarga de una comisión (hito 19).</summary>
public interface ICargaComisionDocenteQuery
{
    /// <summary>
    /// Comisión + cabecera de la carga (si existe) + alumnos cursando/recursando con los
    /// valores actuales de CURSADA y los precargados. Null si la comisión no existe en COMARM.
    /// </summary>
    Task<CargaComisionDocenteDto?> ObtenerAsync(ClaveComision comision, CancellationToken ct);
}
