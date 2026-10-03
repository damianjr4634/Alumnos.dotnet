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
        builder.Property(u => u.PasswordLegacy).HasColumnName("PASSWD").HasMaxLength(60).IsRequired();
        builder.Property(u => u.PasswordHashNuevo).HasColumnName("NPASSWD").HasMaxLength(60);
        builder.Property(u => u.Nombres).HasColumnName("NOMUSU").HasMaxLength(50);
        builder.Property(u => u.Apellido).HasColumnName("APELLIDO").HasMaxLength(50);
        builder.Property(u => u.Cargo).HasColumnName("CARGO").HasMaxLength(30);
        builder.Property(u => u.EsSupervisor).HasColumnName("SUPERV").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(u => u.DebeCambiarPassword).HasColumnName("CAMPASS").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(u => u.SesionUid).HasColumnName("UID").HasMaxLength(50);
        builder.Property(u => u.ImagenFirma).HasColumnName("IMGFIRMA").HasMaxLength(30);
        builder.Property(u => u.FechaBaja).HasColumnName("FECHA_BAJ");

        // Perfil de acceso y vínculo (migración 2026-10-02_usuarios_tipo_vinculo.sql).
        builder.Property(u => u.Tipo).HasColumnName("TIPO").HasMaxLength(3).IsRequired()
            .HasConversion(FbConverters.TipoUsuarioChar);
        builder.Property(u => u.CodigoDocente).HasColumnName("CODPROFES").HasMaxLength(3);
        builder.Property(u => u.AlumnoCarrera).HasColumnName("ALU_CARRE").HasMaxLength(6);
        builder.Property(u => u.AlumnoCodigo).HasColumnName("ALU_COD_ALU").HasMaxLength(11);

        // EstaDeBaja y UsaEscritorio son calculadas: no se mapean.
        builder.Ignore(u => u.EstaDeBaja);
        builder.Ignore(u => u.UsaEscritorio);

        builder.HasMany(u => u.Permisos)
            .WithOne(p => p.Usuario)
            .HasForeignKey(p => p.CodigoUsuario);
    }
}
