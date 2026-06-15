using Esba.Application.DTOs.Asistencias;

namespace Esba.Application.Abstractions;

/// <summary>Escritura de FALTAS (acceso por Dapper: clave única con COD_MAT nullable + reemplazo masivo).</summary>
public interface IInasistenciasRepository
{
    /// <summary>
    /// Reemplaza, en una transacción, todas las faltas de la comisión en el año
    /// indicado por <paramref name="faltas"/> (delete por (carrera, cutuco,
    /// materia, año) + insert de la lista). Devuelve la cantidad insertada.
    /// </summary>
    Task<int> ReemplazarFaltasComisionAsync(
        string codigoCarrera,
        short cutuco,
        string? codigoMateria,
        int anio,
        short? usuario,
        IReadOnlyList<FaltaInasistencia> faltas,
        CancellationToken ct);
}
