using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class CursadaConfiguration : IEntityTypeConfiguration<Cursada>
{
    public void Configure(EntityTypeBuilder<Cursada> builder)
    {
        builder.ToTable("CURSADA");
        builder.HasKey(c => new { c.CodigoCarrera, c.CodigoAlumno, c.CodigoMateria });

        builder.HasOne(c => c.Materia)
            .WithMany()
            .HasForeignKey(c => new { c.CodigoMateria, c.CodigoCarrera });

        builder.Property(c => c.CodigoCarrera).HasColumnName("CARRE").HasMaxLength(6);
        builder.Property(c => c.CodigoAlumno).HasColumnName("COD_ALU").HasMaxLength(11).IsFixedLength();
        builder.Property(c => c.CodigoMateria).HasColumnName("COD_MAT").HasMaxLength(2).IsFixedLength();
        builder.Property(c => c.Apellido).HasColumnName("APELLIDO").HasMaxLength(25);
        builder.Property(c => c.Cutuco).HasColumnName("CUTUCO").HasColumnType("NUMERIC(3,0)");
        builder.Property(c => c.CuatrimestreAnio).HasColumnName("CUA_ANIO").HasMaxLength(3).IsFixedLength();
        builder.Property(c => c.Condicion).HasColumnName("CONDICION").HasMaxLength(15).IsRequired();

        builder.Property(c => c.Evaluacion1).HasColumnName("TP_EVA").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.Recuperatorio1).HasColumnName("RECUP").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.Evaluacion2).HasColumnName("TP_EVA2").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.Recuperatorio2).HasColumnName("RECUP2").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.Evaluacion3).HasColumnName("TP_EVA3").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.NotaRegular).HasColumnName("REGULAR").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.Promedio).HasColumnName("PROM").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.FechaEvaluacion1).HasColumnName("FEC_EVA1");
        builder.Property(c => c.FechaEvaluacion2).HasColumnName("FEC_EVA2");
        builder.Property(c => c.FechaEvaluacion3).HasColumnName("FEC_EVA3");
        builder.Property(c => c.FaltasEvaluacion1).HasColumnName("FAL_EVA1").HasColumnType("NUMERIC(9,2)");
        builder.Property(c => c.FaltasEvaluacion2).HasColumnName("FAL_EVA2").HasColumnType("NUMERIC(9,2)");
        builder.Property(c => c.FaltasEvaluacion3).HasColumnName("FAL_EVA3").HasColumnType("NUMERIC(9,2)");

        builder.Property(c => c.TotalHoras).HasColumnName("TOT_HORAS").HasColumnType("NUMERIC(3,0)");
        builder.Property(c => c.Inasistencias).HasColumnName("INASIST").HasColumnType("NUMERIC(3,0)");
        builder.Property(c => c.Justificadas).HasColumnName("JUSTIF").HasColumnType("NUMERIC(3,0)");

        builder.Property(c => c.NotaFinal1).HasColumnName("FINAL1").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.FechaFinal1).HasColumnName("FECHA1");
        builder.Property(c => c.NotaFinal2).HasColumnName("FINAL2").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.FechaFinal2).HasColumnName("FECHA2");
        builder.Property(c => c.NotaFinal3).HasColumnName("FINAL3").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.FechaFinal3).HasColumnName("FECHA3");
        builder.Property(c => c.NotaFinal4).HasColumnName("FINAL4").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.FechaFinal4).HasColumnName("FECHA4");
        builder.Property(c => c.ActaFinal1).HasColumnName("FACTFIN1").HasMaxLength(10);
        builder.Property(c => c.ActaFinal2).HasColumnName("FACTFIN2").HasMaxLength(10);
        builder.Property(c => c.ActaFinal3).HasColumnName("FACTFIN3").HasMaxLength(10);
        builder.Property(c => c.NotaDiciembre).HasColumnName("NOTADIC").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.FechaDiciembre).HasColumnName("FECHDIC");
        builder.Property(c => c.NotaMarzo).HasColumnName("NOTAMAR").HasColumnType("NUMERIC(5,2)");
        builder.Property(c => c.FechaMarzo).HasColumnName("FECHMAR");

        builder.Property(c => c.Matriz).HasColumnName("MATRIZ").HasMaxLength(5).IsFixedLength();
        builder.Property(c => c.Instituto).HasColumnName("INSTITUT").HasMaxLength(30);
        builder.Property(c => c.Caracteristica).HasColumnName("CARAC").HasMaxLength(5);
        builder.Property(c => c.ActaInterna).HasColumnName("ACTINT").HasMaxLength(15);
        builder.Property(c => c.ActaDge).HasColumnName("ACTDGE").HasMaxLength(15);
        builder.Property(c => c.ActaSne).HasColumnName("ACTSNE").HasMaxLength(7);
        builder.Property(c => c.NumeroRegistro).HasColumnName("NREG").HasColumnType("NUMERIC(5,0)");
        builder.Property(c => c.Colegio).HasColumnName("COLEGIO").HasMaxLength(40);
        builder.Property(c => c.Plan).HasColumnName("PLAN").HasMaxLength(15);
        builder.Property(c => c.Ac).HasColumnName("A_C").HasMaxLength(1);
        builder.Property(c => c.Define).HasColumnName("DEFINE").HasMaxLength(1);

        // Generado por el trigger CURSADA_BI0 con GEN_ID(G_CURSADA).
        builder.Property(c => c.Indice).HasColumnName("INDICE").ValueGeneratedOnAdd();
        builder.Property(c => c.Usuario).HasColumnName("USUARIO").HasMaxLength(15);
        builder.Property(c => c.UltimaModificacion).HasColumnName("ULTMOD").ValueGeneratedOnAddOrUpdate();
    }
}
