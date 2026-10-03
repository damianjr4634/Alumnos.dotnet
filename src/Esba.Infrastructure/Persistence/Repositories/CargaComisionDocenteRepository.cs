using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class CargaComisionDocenteRepository : ICargaComisionDocenteRepository
{
    private readonly EsbaDbContext _contexto;

    public CargaComisionDocenteRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<CargaComisionDocente?> ObtenerPorComisionAsync(ClaveComision comision, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comision);

        // CHAR con relleno: Firebird compara ignorando los espacios finales, así que
        // los valores sin padding matchean igual.
        var carre = comision.CodigoCarrera.Trim();
        var codMat = comision.CodigoMateria.Trim();
        var cuaAnio = comision.CuatrimestreAnio.Trim();
        return _contexto.CargasComisionDocente
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.CodigoCarrera == carre && c.Cutuco == comision.Cutuco
                && c.CodigoMateria == codMat && c.CuatrimestreAnio == cuaAnio, ct);
    }

    public void Agregar(CargaComisionDocente carga) => _contexto.CargasComisionDocente.Add(carga);
}
