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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EsbaDbContext).Assembly);
    }
}
