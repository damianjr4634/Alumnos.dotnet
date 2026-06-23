using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de DOCENTES. Cubre el subconjunto "esencial" del ABM de profesores
/// (hito 10.2): identificación, documento, contacto, domicilio, fechas y
/// licencia. Las columnas de antigüedad docente, sexo/género/nacionalidad, obra
/// social y la tabla de títulos (DOC_TITULOS) quedan sin mapear (deuda).
/// </summary>
public sealed class DocenteConfiguration : IEntityTypeConfiguration<Docente>
{
    public void Configure(EntityTypeBuilder<Docente> builder)
    {
        builder.ToTable("DOCENTES");
        builder.HasKey(d => d.Codigo);

        builder.Property(d => d.Codigo).HasColumnName("CODPROFES").HasMaxLength(3).IsFixedLength();
        builder.Property(d => d.Nombre).HasColumnName("DOCENTE").HasMaxLength(80);
        builder.Property(d => d.TipoDocumento).HasColumnName("TIPODOC").HasMaxLength(3).IsFixedLength();
        builder.Property(d => d.NumeroDocumento).HasColumnName("NRODOCUM").HasMaxLength(8).IsFixedLength();
        builder.Property(d => d.FechaNacimiento).HasColumnName("FEC_NAC");
        builder.Property(d => d.Direccion).HasColumnName("DI_ECCION").HasMaxLength(30).IsFixedLength();
        builder.Property(d => d.Piso).HasColumnName("PISO").HasMaxLength(2).IsFixedLength();
        builder.Property(d => d.Departamento).HasColumnName("DEPTO").HasMaxLength(2).IsFixedLength();
        builder.Property(d => d.CodigoPostal).HasColumnName("COD_POST").HasMaxLength(4).IsFixedLength();
        builder.Property(d => d.Localidad).HasColumnName("LOCALIDAD").HasMaxLength(30).IsFixedLength();
        builder.Property(d => d.TelefonoParticular).HasColumnName("TELEFONO_P").HasMaxLength(20);
        builder.Property(d => d.TelefonoMensajes).HasColumnName("TELEFONO_M").HasMaxLength(20);
        builder.Property(d => d.Interno).HasColumnName("INTERNO").HasMaxLength(4).IsFixedLength();
        builder.Property(d => d.FechaIngreso).HasColumnName("FECHA_ING");
        builder.Property(d => d.FechaBaja).HasColumnName("FECHA_BAJ");
        builder.Property(d => d.EnLicencia).HasColumnName("LICENCIA").HasMaxLength(1).IsFixedLength()
            .HasConversion(FbConverters.SiNoNulable);
        builder.Property(d => d.FechaLicencia).HasColumnName("LICENFECH");

        builder.Ignore(d => d.EstaDeBaja);
    }
}
