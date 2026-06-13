using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface ICursadaRepository
{
    /// <summary>Busca por PK compuesta (CARRE, COD_ALU, COD_MAT), trackeado para edición.</summary>
    Task<Cursada?> ObtenerAsync(string codigoCarrera, string codigoAlumno, string codigoMateria, CancellationToken ct);

    void Agregar(Cursada cursada);

    void Eliminar(Cursada cursada);
}
