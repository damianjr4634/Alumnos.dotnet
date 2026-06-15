using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface IMateriaRepository
{
    /// <summary>Busca por PK compuesta (CODMATERI, CODCARRE), trackeado para edición.</summary>
    Task<Materia?> ObtenerAsync(string codigoMateria, string codigoCarrera, CancellationToken ct);

    void Agregar(Materia materia);
}
