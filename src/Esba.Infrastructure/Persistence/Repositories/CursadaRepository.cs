using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class CursadaRepository : ICursadaRepository
{
    private readonly EsbaDbContext _contexto;

    public CursadaRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Cursada?> ObtenerAsync(string codigoCarrera, string codigoAlumno, string codigoMateria, CancellationToken ct) =>
        _contexto.Cursadas.FirstOrDefaultAsync(
            c => c.CodigoCarrera == codigoCarrera && c.CodigoAlumno == codigoAlumno && c.CodigoMateria == codigoMateria,
            ct);

    public void Agregar(Cursada cursada) => _contexto.Cursadas.Add(cursada);

    public void Eliminar(Cursada cursada) => _contexto.Cursadas.Remove(cursada);
}
