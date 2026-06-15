using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class TipoMesaConfiguration : IEntityTypeConfiguration<TipoMesa>
{
    public void Configure(EntityTypeBuilder<TipoMesa> builder)
    {
        builder.ToTable("MESA_TIPO");
        builder.HasKey(t => t.Codigo);

        builder.Property(t => t.Codigo).HasColumnName("CODIGO").HasMaxLength(2).IsFixedLength();
        builder.Property(t => t.Descripcion).HasColumnName("DESCRI").HasMaxLength(30);
        builder.Property(t => t.Carreras).HasColumnName("CARRE").HasMaxLength(100);
    }
}
