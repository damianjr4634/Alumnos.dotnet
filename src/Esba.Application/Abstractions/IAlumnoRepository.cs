using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface IAlumnoRepository
{
    /// <summary>Busca por PK compuesta (CARRE, COD_ALU), trackeado para edición.</summary>
    Task<Alumno?> ObtenerAsync(string codigoCarrera, string codigo, CancellationToken ct);

    void Agregar(Alumno alumno);
}
