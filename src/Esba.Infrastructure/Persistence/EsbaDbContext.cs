using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Esba.Infrastructure.Persistence;

public class EsbaDbContext : DbContext
{
    public EsbaDbContext(DbContextOptions<EsbaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Alumno> Alumnos => Set<Alumno>();

    public DbSet<Carrera> Carreras => Set<Carrera>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<PermisoUsuario> PermisosUsuario => Set<PermisoUsuario>();

    public DbSet<Materia> Materias => Set<Materia>();

    public DbSet<Comision> Comisiones => Set<Comision>();

    public DbSet<Docente> Docentes => Set<Docente>();

    public DbSet<TipoFalta> TiposFalta => Set<TipoFalta>();

    public DbSet<Mesa> Mesas => Set<Mesa>();

    public DbSet<TipoMesa> TiposMesa => Set<TipoMesa>();

    public DbSet<Cursada> Cursadas => Set<Cursada>();

    public DbSet<Analitico> Analiticos => Set<Analitico>();

    public DbSet<CicloCuatrimestral> CiclosCuatrimestrales => Set<CicloCuatrimestral>();

    public DbSet<CicloTrimestral> CiclosTrimestrales => Set<CicloTrimestral>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EsbaDbContext).Assembly);
    }
}
