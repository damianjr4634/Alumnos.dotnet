using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>Lecturas del catálogo de materias.</summary>
public interface IMateriasQuery
{
    Task<IReadOnlyList<MateriaListItemDto>> ListarPorCarreraAsync(string codigoCarrera, CancellationToken ct);
}
