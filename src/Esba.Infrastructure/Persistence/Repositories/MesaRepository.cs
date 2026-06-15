using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class MesaRepository : IMesaRepository
{
    private readonly EsbaDbContext _contexto;

    public MesaRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Mesa?> ObtenerAsync(string codigoCarrera, int numeroMesa, CancellationToken ct) =>
        _contexto.Mesas.FirstOrDefaultAsync(m => m.CodigoCarrera == codigoCarrera && m.NumeroMesa == numeroMesa, ct);

    public void Agregar(Mesa mesa) => _contexto.Mesas.Add(mesa);

    public void Eliminar(Mesa mesa) => _contexto.Mesas.Remove(mesa);
}
