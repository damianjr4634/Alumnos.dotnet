using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class CarreraConfiguration : IEntityTypeConfiguration<Carrera>
{
    public void Configure(EntityTypeBuilder<Carrera> builder)
    {
        builder.ToTable("CARRERA");
        builder.HasKey(c => c.Codigo);

        builder.Property(c => c.Codigo).HasColumnName("CARRE").HasMaxLength(6);
        builder.Property(c => c.Nombre).HasColumnName("DESCARRE").HasMaxLength(150);
        builder.Property(c => c.Tipo).HasColumnName("TIPO").HasMaxLength(3);
        builder.Property(c => c.Cuatrimestres).HasColumnName("CUATRIM");
        builder.Property(c => c.Resolucion).HasColumnName("RESOLUCION").HasMaxLength(20);
        builder.Property(c => c.Camino).HasColumnName("CAMINO").HasMaxLength(30);
        builder.Property(c => c.NombreAlternativo).HasColumnName("DESCARRE2").HasMaxLength(40);
        builder.Property(c => c.Cuatrim2).HasColumnName("CUATRIM2").HasMaxLength(1);
        builder.Property(c => c.Resolucion2).HasColumnName("RESOLU2").HasMaxLength(15);
        builder.Property(c => c.Rector).HasColumnName("RECTOR").HasMaxLength(50);
        builder.Property(c => c.DniRector).HasColumnName("DNIRECTOR").HasMaxLength(10);
        builder.Property(c => c.Secretaria).HasColumnName("SECRETARIA").HasMaxLength(30);
        builder.Property(c => c.DniSecretaria).HasColumnName("DNISECRET").HasMaxLength(10);
        builder.Property(c => c.Usuario).HasColumnName("USUARIO");
        builder.Property(c => c.Instituto).HasColumnName("INSTITUT").HasMaxLength(30);
        builder.Property(c => c.Caracteristica).HasColumnName("CARACT").HasMaxLength(6);
        builder.Property(c => c.DirectorEstudios).HasColumnName("DIRESTU").HasMaxLength(30).IsRequired();
        builder.Property(c => c.DniDirectorEstudios).HasColumnName("DNIDIRESTU").HasMaxLength(15);
        builder.Property(c => c.Grupo).HasColumnName("GRUPO").HasMaxLength(3).IsRequired();
        builder.Property(c => c.Orden).HasColumnName("ORDEN");
        builder.Property(c => c.Imagen).HasColumnName("IMAGEN");
        builder.Property(c => c.NombreCorto).HasColumnName("DESCORT").HasMaxLength(30);
        builder.Property(c => c.EsADistancia).HasColumnName("DISTANCIA").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(c => c.Desactivada).HasColumnName("DESACT").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(c => c.OrdenInforme).HasColumnName("ORDENINF");
        builder.Property(c => c.Dictamen).HasColumnName("DICTAMEN").HasMaxLength(30);
        builder.Property(c => c.RegSolapa).HasColumnName("REGSOLAPA");
        builder.Property(c => c.Duracion).HasColumnName("DURACION").HasMaxLength(30);
        builder.Property(c => c.Idioma).HasColumnName("IDIOMA").HasMaxLength(50).IsRequired();

        // CASO CHAR(2) no se mapea: columna muerta confirmada (2026-06-12), nullable.
    }
}
