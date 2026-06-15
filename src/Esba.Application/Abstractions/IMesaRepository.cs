using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface IMesaRepository
{
    /// <summary>Busca por PK compuesta (CARRE, MESA), trackeada para edición.</summary>
    Task<Mesa?> ObtenerAsync(string codigoCarrera, int numeroMesa, CancellationToken ct);

    void Agregar(Mesa mesa);

    void Eliminar(Mesa mesa);
}
