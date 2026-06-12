namespace Esba.Application.Abstractions;

/// <summary>
/// Confirma los cambios del caso de uso. Una transacción por caso de uso,
/// abierta y cerrada en Application (migration_improvements.md §1.3) — nunca
/// transacciones de larga vida como las del DataModule legacy.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
