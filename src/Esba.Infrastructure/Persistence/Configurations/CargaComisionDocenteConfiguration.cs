using Esba.Domain.Entities;
using Esba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Esba.Infrastructure.Persistence.Configurations;

/// <summary>DOC_CARGA_COMISION (hito 19). Tabla nueva: sí tiene FK física al detalle.</summary>
public sealed class CargaComisionDocenteConfiguration : IEntityTypeConfiguration<CargaComisionDocente>
{
    internal static readonly ValueConverter<EstadoCargaDocente, string> EstadoChar =
        new(v => EstadoCargaDocenteCodigo.ACodigo(v), v => EstadoCargaDocenteCodigo.DesdeCodigo(v));

    public void Configure(EntityTypeBuilder<CargaComisionDocente> builder)
    {
        builder.ToTable("DOC_CARGA_COMISION");
        builder.HasKey(c => c.Id);

        // Generada por el trigger DOC_CARGA_COMISION_BI0 con GEN_ID(G_DOC_CARGA_COMISION).
        builder.Property(c => c.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        builder.Property(c => c.CodigoCarrera).HasColumnName("CARRE").HasMaxLength(6).IsRequired();
        builder.Property(c => c.Cutuco).HasColumnName("CUTUCO");
        builder.Property(c => c.CodigoMateria).HasColumnName("COD_MAT").HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(c => c.CuatrimestreAnio).HasColumnName("CUA_ANIO").HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(c => c.CodigoDocente).HasColumnName("CODPROFES").HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(c => c.Estado).HasColumnName("ESTADO").HasMaxLength(3).IsFixedLength().IsRequired()
            .HasConversion(EstadoChar);
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

        builder.Ignore(c => c.EditablePorDocente);

        builder.HasMany(c => c.Detalles)
            .WithOne(d => d.Carga)
            .HasForeignKey(d => d.CargaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>DOC_CARGA_COMISION_DET (hito 19).</summary>
public sealed class CargaComisionDocenteDetalleConfiguration : IEntityTypeConfiguration<CargaComisionDocenteDetalle>
{
    public void Configure(EntityTypeBuilder<CargaComisionDocenteDetalle> builder)
    {
        builder.ToTable("DOC_CARGA_COMISION_DET");
        builder.HasKey(d => d.Id);

        // Generada por el trigger DOC_CARGA_COMISION_DET_BI0.
        builder.Property(d => d.Id).HasColumnName("ID").ValueGeneratedOnAdd();
        builder.Property(d => d.CargaId).HasColumnName("CARGA_ID");
        builder.Property(d => d.CodigoAlumno).HasColumnName("COD_ALU").HasMaxLength(11).IsFixedLength().IsRequired();
        builder.Property(d => d.CursadaIndice).HasColumnName("CURSADA_INDICE");

        builder.Property(d => d.Evaluacion1).HasColumnName("TP_EVA").HasColumnType("NUMERIC(5,2)");
        builder.Property(d => d.Recuperatorio1).HasColumnName("RECUP").HasColumnType("NUMERIC(5,2)");
        builder.Property(d => d.Evaluacion2).HasColumnName("TP_EVA2").HasColumnType("NUMERIC(5,2)");
        builder.Property(d => d.Recuperatorio2).HasColumnName("RECUP2").HasColumnType("NUMERIC(5,2)");
        builder.Property(d => d.Evaluacion3).HasColumnName("TP_EVA3").HasColumnType("NUMERIC(5,2)");
        builder.Property(d => d.NotaRegular).HasColumnName("REGULAR").HasColumnType("NUMERIC(5,2)");
        builder.Property(d => d.NotaFinal).HasColumnName("FINAL1").HasColumnType("NUMERIC(5,2)");
        builder.Property(d => d.TotalHoras).HasColumnName("TOT_HORAS").HasColumnType("NUMERIC(3,0)");
        builder.Property(d => d.Inasistencias).HasColumnName("INASIST").HasColumnType("NUMERIC(3,0)");
        builder.Property(d => d.Justificadas).HasColumnName("JUSTIF").HasColumnType("NUMERIC(3,0)");
        builder.Property(d => d.Observaciones).HasColumnName("OBSERV").HasMaxLength(500);

        builder.Property(d => d.FechaModificacion).HasColumnName("FEC_MODIF");
        builder.Property(d => d.CodigoUsuarioModificacion).HasColumnName("CODUSU_MODIF");

        builder.Ignore(d => d.EstaVacio);
    }
}
