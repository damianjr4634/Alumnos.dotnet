using Esba.Application.Abstractions;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly EsbaDbContext _contexto;

    public UsuarioRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Usuario?> ObtenerPorNombreConPermisosAsync(string nombreUsuario, CancellationToken ct) =>
        _contexto.Usuarios
            .Include(u => u.Permisos)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, ct);
}
