using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class ComisionConfiguration : IEntityTypeConfiguration<Comision>
{
    public void Configure(EntityTypeBuilder<Comision> builder)
    {
        builder.ToTable("COMARM");
        builder.HasKey(c => new { c.CodigoCarrera, c.Cutuco, c.CodigoMateria, c.CuatrimestreAnio });

        builder.HasOne(c => c.Materia)
            .WithMany()
            .HasForeignKey(c => new { c.CodigoMateria, c.CodigoCarrera });

        builder.Property(c => c.CodigoCarrera).HasColumnName("CARRE").HasMaxLength(6);
        builder.Property(c => c.Cutuco).HasColumnName("CUTUCO");
        builder.Property(c => c.CodigoMateria).HasColumnName("COD_MAT").HasMaxLength(2).IsFixedLength();
        builder.Property(c => c.CuatrimestreAnio).HasColumnName("CUA_ANIO").HasMaxLength(3).IsFixedLength();
        builder.Property(c => c.CodigoProfesor).HasColumnName("CODPROFES").HasMaxLength(3);
        builder.Property(c => c.Dia1).HasColumnName("DIA1").HasMaxLength(9);
        builder.Property(c => c.Bloque1).HasColumnName("BLOQUE1").HasMaxLength(7);
        builder.Property(c => c.Dia2).HasColumnName("DIA2").HasMaxLength(9);
        builder.Property(c => c.Bloque2).HasColumnName("BLOQUE2").HasMaxLength(7);
        builder.Property(c => c.Dia3).HasColumnName("DIA3").HasMaxLength(9);
        builder.Property(c => c.Bloque3).HasColumnName("BLOQUE3").HasMaxLength(7);
        builder.Property(c => c.TitularSuplente).HasColumnName("TIT_SUP").HasMaxLength(2);
        builder.Property(c => c.Usuario).HasColumnName("USUARIO").HasMaxLength(15);
    }
}
