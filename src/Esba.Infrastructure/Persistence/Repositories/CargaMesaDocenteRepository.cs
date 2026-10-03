using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class CargaMesaDocenteRepository : ICargaMesaDocenteRepository
{
    private readonly EsbaDbContext _contexto;

    public CargaMesaDocenteRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<CargaMesaDocente?> ObtenerPorMesaAsync(ClaveMesa mesa, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(mesa);

        var carre = mesa.CodigoCarrera.Trim();
        return _contexto.CargasMesaDocente
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.CodigoCarrera == carre && c.NumeroMesa == mesa.NumeroMesa, ct);
    }

    public void Agregar(CargaMesaDocente carga) => _contexto.CargasMesaDocente.Add(carga);
}
