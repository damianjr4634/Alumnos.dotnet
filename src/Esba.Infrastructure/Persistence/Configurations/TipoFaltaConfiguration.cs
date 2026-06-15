using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class TipoFaltaConfiguration : IEntityTypeConfiguration<TipoFalta>
{
    public void Configure(EntityTypeBuilder<TipoFalta> builder)
    {
        builder.ToTable("TBL_FALTAS");
        builder.HasKey(t => t.Codigo);

        builder.Property(t => t.Codigo).HasColumnName("FCODIGO").HasMaxLength(2);
        builder.Property(t => t.Descripcion).HasColumnName("FDESCRI").HasMaxLength(30);
        builder.Property(t => t.Cantidad).HasColumnName("FCANTID").HasPrecision(5, 2);
        builder.Property(t => t.Justifica).HasColumnName("FJUSTIF").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(t => t.Tipo).HasColumnName("FTIPO").HasMaxLength(2);
        builder.Property(t => t.Carreras).HasColumnName("CARRE").HasMaxLength(100);
    }
}
