using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

/// <summary>DOC_CARGA_MESA (hito 19). Tabla nueva: sí tiene FK física al detalle.</summary>
public sealed class CargaMesaDocenteConfiguration : IEntityTypeConfiguration<CargaMesaDocente>
{
    public void Configure(EntityTypeBuilder<CargaMesaDocente> builder)
    {
        builder.ToTable("DOC_CARGA_MESA");
        builder.HasKey(c => c.Id);

        // Generada por el trigger DOC_CARGA_MESA_BI0 con GEN_ID(G_DOC_CARGA_MESA).
        builder.Property(c => c.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        builder.Property(c => c.CodigoCarrera).HasColumnName("CARRE").HasMaxLength(6).IsRequired();
        builder.Property(c => c.NumeroMesa).HasColumnName("MESA");
        builder.Property(c => c.CodigoDocente).HasColumnName("CODPROFES").HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(c => c.Estado).HasColumnName("ESTADO").HasMaxLength(3).IsFixedLength().IsRequired()
            .HasConversion(CargaComisionDocenteConfiguration.EstadoChar);
        builder.Property(c => c.Observaciones).HasColumnName("OBSERV").HasMaxLength(500);

        builder.Property(c => c.FechaAlta).HasColumnName("FEC_ALTA");
        builder.Property(c => c.CodigoUsuarioAlta).HasColumnName("CODUSU_ALTA");
        builder.Property(c => c.FechaModificacion).HasColumnName("FEC_MODIF");
        builder.Property(c => c.CodigoUsuarioModificacion).HasColumnName("CODUSU_MODIF");
        builder.Property(c => c.FechaFinalizacion).HasColumnName("FEC_FINAL");
        builder.Property(c => c.CodigoUsuarioFinalizacion).HasColumnName("CODUSU_FINAL");
        builder.Property(c => c.FechaReapertura).HasColumnName("FEC_REAPERTURA");
        builder.Property(c => c.CodigoUsuarioReapertura).HasColumnName("CODUSU_REAPERTURA");
        builder.Property(c => c.FechaEfectivizacion).HasColumnName("FEC_EFECTIVO");
        builder.Property(c => c.CodigoUsuarioEfectivizacion).HasColumnName("CODUSU_EFECTIVO");

        builder.HasMany(c => c.Detalles)
            .WithOne(d => d.Carga)
            .HasForeignKey(d => d.CargaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>DOC_CARGA_MESA_DET (hito 19).</summary>
public sealed class CargaMesaDocenteDetalleConfiguration : IEntityTypeConfiguration<CargaMesaDocenteDetalle>
{
    public void Configure(EntityTypeBuilder<CargaMesaDocenteDetalle> builder)
    {
        builder.ToTable("DOC_CARGA_MESA_DET");
        builder.HasKey(d => d.Id);

        // Generada por el trigger DOC_CARGA_MESA_DET_BI0.
        builder.Property(d => d.Id).HasColumnName("ID").ValueGeneratedOnAdd();
        builder.Property(d => d.CargaId).HasColumnName("CARGA_ID");
        builder.Property(d => d.CodigoAlumno).HasColumnName("COD_ALU").HasMaxLength(11).IsFixedLength().IsRequired();
        builder.Property(d => d.CodigoMateria).HasColumnName("COD_MAT").HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(d => d.PermisoIndice).HasColumnName("PERMEXA_INDICE");
        builder.Property(d => d.Nota).HasColumnName("NOTA").HasColumnType("NUMERIC(5,2)");
        builder.Property(d => d.Ausente).HasColumnName("AUSENTE").HasMaxLength(1).IsFixedLength()
            .HasConversion(FbConverters.SiNo);
        builder.Property(d => d.Observaciones).HasColumnName("OBSERV").HasMaxLength(500);
        builder.Property(d => d.FechaModificacion).HasColumnName("FEC_MODIF");
        builder.Property(d => d.CodigoUsuarioModificacion).HasColumnName("CODUSU_MODIF");

        builder.Ignore(d => d.EstaVacio);
    }
}
