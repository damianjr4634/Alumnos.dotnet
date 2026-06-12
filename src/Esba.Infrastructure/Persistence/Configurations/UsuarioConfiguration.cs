using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("USUARIOS");
        builder.HasKey(u => u.Codigo);

        // Generada por el trigger USUARIOS_BI0 con GEN_ID(G_USUARIOS).
        // TODO-confirmar-identidad: verificar que el provider recupere el valor tras el INSERT.
        builder.Property(u => u.Codigo).HasColumnName("CODUSU").ValueGeneratedOnAdd();

        builder.Property(u => u.NombreUsuario).HasColumnName("NOMBRE").HasMaxLength(15).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("PASSWD").HasMaxLength(60).IsRequired();
        builder.Property(u => u.Nombres).HasColumnName("NOMUSU").HasMaxLength(50);
        builder.Property(u => u.Apellido).HasColumnName("APELLIDO").HasMaxLength(50);
        builder.Property(u => u.Cargo).HasColumnName("CARGO").HasMaxLength(30);
        builder.Property(u => u.EsSupervisor).HasColumnName("SUPERV").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(u => u.DebeCambiarPassword).HasColumnName("CAMPASS").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(u => u.SesionUid).HasColumnName("UID").HasMaxLength(50);
        builder.Property(u => u.ImagenFirma).HasColumnName("IMGFIRMA").HasMaxLength(30);

        builder.HasMany(u => u.Permisos)
            .WithOne(p => p.Usuario)
            .HasForeignKey(p => p.CodigoUsuario);
    }
}
