using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class CicloLectivoRepository : ICicloLectivoRepository
{
    private readonly EsbaDbContext _contexto;

    public CicloLectivoRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<CicloCuatrimestral?> ObtenerCuatrimestralAsync(int anio, CancellationToken ct) =>
        _contexto.CiclosCuatrimestrales.FirstOrDefaultAsync(c => c.Anio == anio, ct);

    public Task<CicloTrimestral?> ObtenerTrimestralAsync(int anio, CancellationToken ct) =>
        _contexto.CiclosTrimestrales.FirstOrDefaultAsync(c => c.Anio == anio, ct);

    public void Agregar(CicloCuatrimestral ciclo) => _contexto.CiclosCuatrimestrales.Add(ciclo);

    public void Agregar(CicloTrimestral ciclo) => _contexto.CiclosTrimestrales.Add(ciclo);

    public void Eliminar(CicloCuatrimestral ciclo) => _contexto.CiclosCuatrimestrales.Remove(ciclo);

    public void Eliminar(CicloTrimestral ciclo) => _contexto.CiclosTrimestrales.Remove(ciclo);
}
