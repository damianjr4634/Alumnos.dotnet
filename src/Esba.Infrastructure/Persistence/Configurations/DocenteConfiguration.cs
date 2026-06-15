using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo mínimo de DOCENTES (hito 6): solo las columnas necesarias para el join
/// de comisiones y el combo de docente. El resto de las columnas de la tabla se
/// mapearán en el ABM de profesores (hito 10).
/// </summary>
public sealed class DocenteConfiguration : IEntityTypeConfiguration<Docente>
{
    public void Configure(EntityTypeBuilder<Docente> builder)
    {
        builder.ToTable("DOCENTES");
        builder.HasKey(d => d.Codigo);

        builder.Property(d => d.Codigo).HasColumnName("CODPROFES").HasMaxLength(3).IsFixedLength();
        builder.Property(d => d.Nombre).HasColumnName("DOCENTE").HasMaxLength(80);
        builder.Property(d => d.FechaIngreso).HasColumnName("FECHA_ING");
        builder.Property(d => d.FechaBaja).HasColumnName("FECHA_BAJ");

        // El resto de las columnas de DOCENTES queda fuera del modelo hasta el
        // ABM de profesores (hito 10); el esquema se versiona por DDL, no por
        // migraciones EF, así que el mapeo parcial es seguro.
    }
}
