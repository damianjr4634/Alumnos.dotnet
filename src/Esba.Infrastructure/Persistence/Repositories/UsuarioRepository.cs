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

    public Task<Usuario?> ObtenerPorCodigoAsync(int codigo, CancellationToken ct) =>
        _contexto.Usuarios.FirstOrDefaultAsync(u => u.Codigo == codigo, ct);

    public Task<bool> ExisteNombreAsync(string nombreUsuario, int? codigoExcluido, CancellationToken ct)
    {
        var nombre = nombreUsuario.ToUpperInvariant();
#pragma warning disable CA1304, CA1311, CA1862 // EF traduce ToUpper() a UPPER() de Firebird.
        return _contexto.Usuarios
            .AnyAsync(u => u.NombreUsuario.ToUpper() == nombre
                && (codigoExcluido == null || u.Codigo != codigoExcluido), ct);
#pragma warning restore CA1304, CA1311, CA1862
    }

    public Task<int> ContarSupervisoresActivosAsync(CancellationToken ct) =>
        _contexto.Usuarios.CountAsync(u => u.EsSupervisor && u.FechaBaja == null, ct);

    public void Agregar(Usuario usuario) => _contexto.Usuarios.Add(usuario);
}
