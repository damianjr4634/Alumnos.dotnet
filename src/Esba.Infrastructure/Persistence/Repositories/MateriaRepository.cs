using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class MateriaRepository : IMateriaRepository
{
    private readonly EsbaDbContext _contexto;

    public MateriaRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Materia?> ObtenerAsync(string codigoMateria, string codigoCarrera, CancellationToken ct) =>
        _contexto.Materias.FirstOrDefaultAsync(
            m => m.Codigo == codigoMateria && m.CodigoCarrera == codigoCarrera, ct);
}
