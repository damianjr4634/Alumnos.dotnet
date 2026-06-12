using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class PermisoUsuarioConfiguration : IEntityTypeConfiguration<PermisoUsuario>
{
    public void Configure(EntityTypeBuilder<PermisoUsuario> builder)
    {
        builder.ToTable("BARRA_SEGU");

        // PK física compuesta; como en el resto del esquema legacy, no hay FK
        // declarada hacia USUARIOS: la relación se modela solo en EF.
        builder.HasKey(p => new { p.CodigoUsuario, p.CodigoOpcion });

        builder.Property(p => p.CodigoUsuario).HasColumnName("CODUSU");
        builder.Property(p => p.CodigoOpcion).HasColumnName("BAROPC").HasMaxLength(6);
    }
}
