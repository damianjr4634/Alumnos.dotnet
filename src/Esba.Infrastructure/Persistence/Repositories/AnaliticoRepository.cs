using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class AnaliticoRepository : IAnaliticoRepository
{
    private readonly EsbaDbContext _contexto;

    public AnaliticoRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Analitico?> ObtenerAsync(string codigoCarrera, string codigoAlumno, string codigoMateria, CancellationToken ct) =>
        _contexto.Analiticos.FirstOrDefaultAsync(
            a => a.CodigoCarrera == codigoCarrera && a.CodigoAlumno == codigoAlumno && a.CodigoMateria == codigoMateria,
            ct);

    public void Agregar(Analitico analitico) => _contexto.Analiticos.Add(analitico);

    public void Eliminar(Analitico analitico) => _contexto.Analiticos.Remove(analitico);
}
