using Esba.Domain.Entities;

namespace Esba.Application.Abstractions;

public interface IDocenteRepository
{
    /// <summary>Busca por PK (CODPROFES), trackeado para edición/baja.</summary>
    Task<Docente?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct);

    /// <summary>true si ya existe un docente con ese código.</summary>
    Task<bool> ExisteAsync(string codigo, CancellationToken ct);

    void Agregar(Docente docente);
}
