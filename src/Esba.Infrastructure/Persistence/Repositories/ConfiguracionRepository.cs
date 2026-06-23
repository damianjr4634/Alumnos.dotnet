using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly EsbaDbContext _contexto;

    public ConfiguracionRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<ParametroConfiguracion>> ObtenerPorParamesAsync(
        IReadOnlyCollection<string> parames, CancellationToken ct)
    {
        if (parames.Count == 0)
        {
            return [];
        }

        return await _contexto.Configuraciones
            .Where(p => parames.Contains(p.Parame))
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
