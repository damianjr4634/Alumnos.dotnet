using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class DocenteRepository : IDocenteRepository
{
    private readonly EsbaDbContext _contexto;

    public DocenteRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Docente?> ObtenerPorCodigoAsync(string codigo, CancellationToken ct) =>
        _contexto.Docentes.FirstOrDefaultAsync(d => d.Codigo == codigo, ct);

    public Task<bool> ExisteAsync(string codigo, CancellationToken ct) =>
        _contexto.Docentes.AnyAsync(d => d.Codigo == codigo, ct);

    public void Agregar(Docente docente) => _contexto.Docentes.Add(docente);
}
