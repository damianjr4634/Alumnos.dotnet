namespace Esba.Application.Common;

/// <summary>
/// Página de resultados para grillas server-side (la paginación se resuelve en
/// Firebird, nunca en memoria — migration_improvements.md §3.2).
/// </summary>
public sealed record PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>Total de filas que matchean el filtro, sin paginar.</summary>
    public required int Total { get; init; }
}
