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

    public Task<Usuario?> ObtenerPorNombreConPermisosAsync(string nombreUsuario, CancellationToken ct)
    {
        // Insensible a mayúsculas: el login legacy forzaba ecUpperCase en el
        // TEdit (sesion.dfm) antes de su comparación exacta — acá se resuelve
        // en la consulta (UPPER en ambos lados).
        var nombre = nombreUsuario.ToUpperInvariant();
#pragma warning disable CA1304, CA1311, CA1862 // ToUpper() no se ejecuta en .NET: EF lo traduce a UPPER() de Firebird.
        return _contexto.Usuarios
            .Include(u => u.Permisos)
            .FirstOrDefaultAsync(u => u.NombreUsuario.ToUpper() == nombre, ct);
#pragma warning restore CA1304, CA1311, CA1862
    }
}
