using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface IAnaliticoRepository
{
    /// <summary>Busca por PK compuesta (CARRE, COD_ALU, COD_MAT), trackeado para edición.</summary>
    Task<Analitico?> ObtenerAsync(string codigoCarrera, string codigoAlumno, string codigoMateria, CancellationToken ct);

    void Agregar(Analitico analitico);

    void Eliminar(Analitico analitico);
}
