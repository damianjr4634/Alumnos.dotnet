using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class MateriaConfiguration : IEntityTypeConfiguration<Materia>
{
    public void Configure(EntityTypeBuilder<Materia> builder)
    {
        builder.ToTable("MATERIAS");
        builder.HasKey(m => new { m.Codigo, m.CodigoCarrera });

        builder.HasOne(m => m.Carrera)
            .WithMany()
            .HasForeignKey(m => m.CodigoCarrera);

        builder.Property(m => m.Codigo).HasColumnName("CODMATERI").HasMaxLength(2).IsFixedLength();
        builder.Property(m => m.CodigoCarrera).HasColumnName("CODCARRE").HasMaxLength(6);
        builder.Property(m => m.Nombre).HasColumnName("DESCRIPCI").HasMaxLength(60);
        builder.Property(m => m.Sigla).HasColumnName("SIGLA").HasMaxLength(30);
        builder.Property(m => m.Cuatrimestre).HasColumnName("CUATRIM");
        builder.Property(m => m.CodigoEquivalencia).HasColumnName("EQUIVALE").HasMaxLength(2);
        builder.Property(m => m.CorrelativasCursada).HasColumnName("CORRELATIV").HasMaxLength(100);
        builder.Property(m => m.CorrelativasFinal).HasColumnName("CORREFINAL").HasMaxLength(100);
        builder.Property(m => m.EsLaboratorio).HasColumnName("LAB").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(m => m.Estado).HasColumnName("ESTADO").HasMaxLength(1);
        builder.Property(m => m.EsAnual).HasColumnName("ANUAL").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(m => m.CodigoAnual).HasColumnName("CODANUAL").HasMaxLength(2);
        builder.Property(m => m.CodigoNuevo).HasColumnName("CODNEW").HasMaxLength(2);
        builder.Property(m => m.AdmitePromocion).HasColumnName("PROMOCION").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(m => m.ApruebaSinFinal).HasColumnName("APRSFINAL").HasMaxLength(1);
        builder.Property(m => m.AptoSinFinal).HasColumnName("APTSFINAL").HasMaxLength(5);
        builder.Property(m => m.Orden).HasColumnName("ORDEN");
        builder.Property(m => m.Usuario).HasColumnName("USUARIO").HasMaxLength(15);
    }
}
