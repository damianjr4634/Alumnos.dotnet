using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("MESAS");
        builder.HasKey(m => new { m.CodigoCarrera, m.NumeroMesa });

        builder.Property(m => m.CodigoCarrera).HasColumnName("CARRE").HasMaxLength(6);
        builder.Property(m => m.NumeroMesa).HasColumnName("MESA");
        builder.Property(m => m.CodigoMateria).HasColumnName("COD_MAT").HasMaxLength(2).IsFixedLength();
        builder.Property(m => m.Llamado).HasColumnName("LLAMADO");
        builder.Property(m => m.FechaExamen).HasColumnName("FECH_EXA");
        builder.Property(m => m.Hora).HasColumnName("HORA");
        builder.Property(m => m.Titular).HasColumnName("TITULAR").HasMaxLength(3).IsFixedLength();
        builder.Property(m => m.Vocal1).HasColumnName("VOCAL1").HasMaxLength(3).IsFixedLength();
        builder.Property(m => m.Vocal2).HasColumnName("VOCAL2").HasMaxLength(3).IsFixedLength();
        builder.Property(m => m.Aula).HasColumnName("AULA");
        builder.Property(m => m.Cuatrimestre).HasColumnName("CUATRIM");
        builder.Property(m => m.Comision1).HasColumnName("COMI1");
        builder.Property(m => m.Comision2).HasColumnName("COMI2");
        builder.Property(m => m.Comision3).HasColumnName("COMI3");
        builder.Property(m => m.CodigoTipo).HasColumnName("TIPMES").HasMaxLength(2).IsFixedLength();
        builder.Property(m => m.Usuario).HasColumnName("USUARIO").HasMaxLength(15).IsFixedLength();
        builder.Property(m => m.NumeroRegistro).HasColumnName("NREG");
        builder.Property(m => m.UltimaModificacion).HasColumnName("ULTMOD");
    }
}
