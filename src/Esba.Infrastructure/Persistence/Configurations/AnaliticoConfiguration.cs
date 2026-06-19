using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class AnaliticoConfiguration : IEntityTypeConfiguration<Analitico>
{
    public void Configure(EntityTypeBuilder<Analitico> builder)
    {
        builder.ToTable("ANALITIC");
        builder.HasKey(a => new { a.CodigoCarrera, a.CodigoAlumno, a.CodigoMateria });

        builder.HasOne(a => a.Materia)
            .WithMany()
            .HasForeignKey(a => new { a.CodigoMateria, a.CodigoCarrera });

        builder.Property(a => a.CodigoCarrera).HasColumnName("CARRE").HasMaxLength(6);
        builder.Property(a => a.CodigoAlumno).HasColumnName("COD_ALU").HasMaxLength(11).IsFixedLength();
        builder.Property(a => a.CodigoMateria).HasColumnName("COD_MAT").HasMaxLength(2).IsFixedLength();
        builder.Property(a => a.Apellido).HasColumnName("APELLIDO").HasMaxLength(25).IsFixedLength();
        builder.Property(a => a.CuatrimestreAnio).HasColumnName("CUA_ANIO").HasMaxLength(3).IsFixedLength();
        builder.Property(a => a.Nota).HasColumnName("NOTA_MAT").HasColumnType("NUMERIC(5,2)");
        builder.Property(a => a.FechaFinal).HasColumnName("FEC_FINAL");
        builder.Property(a => a.Condicion).HasColumnName("CONDICION").HasMaxLength(15).IsFixedLength();
        builder.Property(a => a.Matriz).HasColumnName("MATRIZ").HasMaxLength(5).IsFixedLength();

        builder.Property(a => a.Instituto).HasColumnName("INSTITUT").HasMaxLength(30).IsFixedLength();
        builder.Property(a => a.Caracteristica).HasColumnName("CARAC").HasMaxLength(6).IsFixedLength();
        builder.Property(a => a.ActaInterna).HasColumnName("ACTINT").HasMaxLength(15);
        builder.Property(a => a.ActaDge).HasColumnName("ACTDGE").HasMaxLength(15);
        builder.Property(a => a.ActaSne).HasColumnName("ACTSNE").HasMaxLength(10);
        builder.Property(a => a.Colegio).HasColumnName("COLEGIO").HasMaxLength(40).IsFixedLength();
        builder.Property(a => a.Plan).HasColumnName("PLAN").HasMaxLength(40).IsFixedLength();
        builder.Property(a => a.Ac).HasColumnName("A_C").HasMaxLength(1).IsFixedLength();
        builder.Property(a => a.NumeroRegistro).HasColumnName("NREG").HasColumnType("NUMERIC(5,0)");
        builder.Property(a => a.EquivDocente).HasColumnName("FEQDOCE").HasMaxLength(3);
        builder.Property(a => a.EquivMateria).HasColumnName("FEQMATE").HasMaxLength(50);
        builder.Property(a => a.EquivCarrera).HasColumnName("FEQCARRE").HasMaxLength(100);
        builder.Property(a => a.EquivInstituto).HasColumnName("FEQINST").HasMaxLength(100);
        builder.Property(a => a.ActaFinal).HasColumnName("FACTFIN").HasMaxLength(10);
        builder.Property(a => a.EximidoDescripcion).HasColumnName("FEXDESCRI").HasMaxLength(200);

        // Generado por el trigger ANALITIC_BI0 con GEN_ID(G_ANALITIC).
        builder.Property(a => a.Indice).HasColumnName("INDICE").ValueGeneratedOnAdd();
        builder.Property(a => a.Usuario).HasColumnName("USUARIO").HasMaxLength(15);
        builder.Property(a => a.UltimaModificacion).HasColumnName("ULTMOD").ValueGeneratedOnAddOrUpdate();
    }
}
