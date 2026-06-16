using Esba.Application.DTOs.Certificados;

namespace Esba.Application.Abstractions;

/// <summary>
/// Lecturas de apoyo para emitir constancias de alumno (sucesoras de las consultas
/// embebidas de constanciaalumnos2.pas sobre CARRERA).
/// </summary>
public interface IConstanciasQuery
{
    /// <summary>
    /// Datos de la carrera para el encabezado y las firmas de la constancia.
    /// null si la carrera no existe.
    /// </summary>
    Task<CarreraConstanciaDto?> ObtenerDatosCarreraAsync(string codigoCarrera, CancellationToken ct);
}
