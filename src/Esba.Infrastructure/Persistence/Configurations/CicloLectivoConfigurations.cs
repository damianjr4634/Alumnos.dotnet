using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class CicloCuatrimestralConfiguration : IEntityTypeConfiguration<CicloCuatrimestral>
{
    public void Configure(EntityTypeBuilder<CicloCuatrimestral> builder)
    {
        builder.ToTable("TBL_CUAT");
        builder.HasKey(c => c.Anio);

        builder.Property(c => c.Anio).HasColumnName("FANIO").ValueGeneratedNever();
        builder.Property(c => c.PrimerCuatrimestreDesde).HasColumnName("FDDEPRI");
        builder.Property(c => c.PrimerCuatrimestreHasta).HasColumnName("FHTAPRI");
        builder.Property(c => c.SegundoCuatrimestreDesde).HasColumnName("FDDESEG");
        builder.Property(c => c.SegundoCuatrimestreHasta).HasColumnName("FHTASEG");
    }
}

public sealed class CicloTrimestralConfiguration : IEntityTypeConfiguration<CicloTrimestral>
{
    public void Configure(EntityTypeBuilder<CicloTrimestral> builder)
    {
        builder.ToTable("TBL_TRIM");
        builder.HasKey(c => c.Anio);

        builder.Property(c => c.Anio).HasColumnName("FANIO").ValueGeneratedNever();
        builder.Property(c => c.PrimerTrimestreDesde).HasColumnName("FDDEPRI");
        builder.Property(c => c.PrimerTrimestreHasta).HasColumnName("FHTAPRI");
        builder.Property(c => c.SegundoTrimestreDesde).HasColumnName("FDDESEG");
        builder.Property(c => c.SegundoTrimestreHasta).HasColumnName("FHTASEG");
        builder.Property(c => c.TercerTrimestreDesde).HasColumnName("FDDETER");
        builder.Property(c => c.TercerTrimestreHasta).HasColumnName("FHTATER");
    }
}
