using Esba.Application.Abstractions;

namespace Esba.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly EsbaDbContext _contexto;

    public EfUnitOfWork(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _contexto.SaveChangesAsync(ct);
}
