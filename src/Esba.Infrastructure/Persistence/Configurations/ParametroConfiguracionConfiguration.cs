using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de XXX_CONF (configuración clave-valor del sistema). PARAME es la PK
/// (no es autogenerada: el código del parámetro lo define quien lo crea).
/// </summary>
public sealed class ParametroConfiguracionConfiguration : IEntityTypeConfiguration<ParametroConfiguracion>
{
    public void Configure(EntityTypeBuilder<ParametroConfiguracion> builder)
    {
        builder.ToTable("XXX_CONF");
        builder.HasKey(p => p.Parame);

        builder.Property(p => p.Parame).HasColumnName("PARAME").HasMaxLength(30).ValueGeneratedNever();
        builder.Property(p => p.Descripcion).HasColumnName("DESCRI").HasMaxLength(100);
        builder.Property(p => p.Valor).HasColumnName("VALOR").HasMaxLength(200);
    }
}
