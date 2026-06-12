using Esba.Domain.Entities;
using Esba.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Esba.Infrastructure.Persistence.Configurations;

public sealed class AlumnoConfiguration : IEntityTypeConfiguration<Alumno>
{
    public void Configure(EntityTypeBuilder<Alumno> builder)
    {
        builder.ToTable("ALUMNOS");

        // PK física compuesta (PK_ALUMNOS). No existe FK real hacia CARRERA en
        // la base legacy: la relación se modela solo en EF para los joins.
        builder.HasKey(a => new { a.CodigoCarrera, a.Codigo });

        builder.HasOne(a => a.Carrera)
            .WithMany()
            .HasForeignKey(a => a.CodigoCarrera);

        builder.Property(a => a.Codigo).HasColumnName("COD_ALU").HasMaxLength(11).IsFixedLength();
        builder.Property(a => a.CodigoCarrera).HasColumnName("CARRE").HasMaxLength(6);
        builder.Property(a => a.Matriz).HasColumnName("MATRIZ").HasMaxLength(5).IsFixedLength();
        builder.Property(a => a.Apellido).HasColumnName("APELLIDO").HasMaxLength(25);
        builder.Property(a => a.Nombre).HasColumnName("NOM_APE").HasMaxLength(25);
        builder.Property(a => a.DocumentoExpedidoPor).HasColumnName("EXT_POR").HasMaxLength(10);

        builder.Property(a => a.Sexo).HasColumnName("SEXO").HasMaxLength(1)
            .HasConversion(FbConverters.SexoChar);
        builder.Property(a => a.Nacionalidad).HasColumnName("NACIONAL").HasMaxLength(15);
        builder.Property(a => a.EstadoCivil).HasColumnName("EST_CIV").HasMaxLength(1)
            .HasConversion(FbConverters.EstadoCivilChar);
        builder.Property(a => a.FechaNacimiento).HasColumnName("FEC_NAC");
        builder.Property(a => a.LugarNacimiento).HasColumnName("LUG_NAC").HasMaxLength(15);
        builder.Property(a => a.ProvinciaNacimiento).HasColumnName("PCIA_NAC").HasMaxLength(20);
        builder.Property(a => a.Domicilio).HasColumnName("DOMI").HasMaxLength(30);
        builder.Property(a => a.Localidad).HasColumnName("LOCALI").HasMaxLength(40);
        builder.Property(a => a.CodigoPostal).HasColumnName("COD_POS").HasColumnType("NUMERIC(4,0)");
        builder.Property(a => a.CaracteristicaTelefono).HasColumnName("CAR_TEL").HasMaxLength(7);
        builder.Property(a => a.Telefono).HasColumnName("TELE").HasMaxLength(15);
        builder.Property(a => a.FechaIngreso).HasColumnName("FEC_ING");

        builder.Property(a => a.ColegioPrimario).HasColumnName("CPRIM").HasMaxLength(50);
        builder.Property(a => a.AnioPrimario).HasColumnName("APRIM").HasMaxLength(2);
        builder.Property(a => a.TituloPrimario).HasColumnName("TPRIM").HasMaxLength(50);
        builder.Property(a => a.ObservacionPrimario).HasColumnName("OPRIM").HasMaxLength(10);
        builder.Property(a => a.ColegioSecundario).HasColumnName("CSECU").HasMaxLength(50);
        builder.Property(a => a.AnioSecundario).HasColumnName("ASECU").HasMaxLength(2);
        builder.Property(a => a.TituloSecundario).HasColumnName("TSECU").HasMaxLength(50);
        builder.Property(a => a.ObservacionSecundario).HasColumnName("OSECU").HasMaxLength(10);
        builder.Property(a => a.InstitucionTerciaria).HasColumnName("TERCI").HasMaxLength(50);
        builder.Property(a => a.AnioTerciario).HasColumnName("ATERCI").HasMaxLength(2);
        builder.Property(a => a.TituloTerciario).HasColumnName("TTERCI").HasMaxLength(50);
        builder.Property(a => a.ObservacionTerciario).HasColumnName("OTERCI").HasMaxLength(10);

        builder.Property(a => a.Empresa).HasColumnName("EMPRE").HasMaxLength(20);
        builder.Property(a => a.Rubro).HasColumnName("RUBRO").HasMaxLength(20);
        builder.Property(a => a.Cargo).HasColumnName("CARGO").HasMaxLength(20);
        builder.Property(a => a.Antiguedad).HasColumnName("ANTI").HasMaxLength(10);
        builder.Property(a => a.DomicilioLaboral).HasColumnName("DOMI_1").HasMaxLength(25);
        builder.Property(a => a.TelefonoLaboral).HasColumnName("TELE_1").HasMaxLength(8);
        builder.Property(a => a.InternoLaboral).HasColumnName("INTER").HasMaxLength(5);

        builder.Property(a => a.Pn).HasColumnName("PN").HasMaxLength(1);
        builder.Property(a => a.TieneCertificadoEnTramite).HasColumnName("CTT").HasMaxLength(1)
            .HasConversion(FbConverters.Asterisco);
        builder.Property(a => a.FechaCertificadoEnTramite).HasColumnName("FECH_CTT");
        builder.Property(a => a.SituacionDni).HasColumnName("DNI").HasMaxLength(1)
            .HasConversion(FbConverters.SituacionDniChar);
        builder.Property(a => a.PresentoCa).HasColumnName("CA").HasMaxLength(1)
            .HasConversion(FbConverters.Asterisco);
        builder.Property(a => a.Cr).HasColumnName("CR").HasMaxLength(1);

        builder.Property(a => a.Baja).HasColumnName("BAJA").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);

        // Generado por el trigger ALUMNOS_BI0 con GEN_ID(G_ALUMNOS).
        // TODO-confirmar-identidad: verificar que el provider recupere el valor
        // tras el INSERT; si no, leerlo explícitamente en el repositorio.
        builder.Property(a => a.Indice).HasColumnName("INDICE").ValueGeneratedOnAdd();
        builder.HasIndex(a => a.Indice).IsUnique();

        builder.Property(a => a.Mail).HasColumnName("MAIL").HasMaxLength(80);
        builder.Property(a => a.Observaciones).HasColumnName("OBSERV").HasMaxLength(32762);
        builder.Property(a => a.Usuario).HasColumnName("USUARIO").HasMaxLength(15);
        builder.Property(a => a.Celular).HasColumnName("CELULAR").HasMaxLength(20);
        builder.Property(a => a.PresentoFoto).HasColumnName("FFOTO").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(a => a.PresentoAptoFisico).HasColumnName("FAPTFIS").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(a => a.FechaAptoFisico).HasColumnName("FAPTFEC");
        builder.Property(a => a.UsuarioWeb).HasColumnName("FUSUWEB").HasMaxLength(20);
        builder.Property(a => a.Foto).HasColumnName("FOTO");
        builder.Property(a => a.PresentoPartidaNacimiento).HasColumnName("FPARNAC").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);

        // Mantenida por los triggers ALUMNOS_BI0/BU0 (CURRENT_TIMESTAMP).
        builder.Property(a => a.UltimaModificacion).HasColumnName("ULTMOD")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(a => a.PresentoLibreta).HasColumnName("FLIBRETA").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);
        builder.Property(a => a.FechaLibreta).HasColumnName("FLIBFEC");
        builder.Property(a => a.Estado).HasColumnName("ESTADO").HasMaxLength(1)
            .HasConversion(FbConverters.EstadoAlumnoChar);
        builder.Property(a => a.Genero).HasColumnName("GENERO").HasConversion<int>();
        builder.Property(a => a.NominaPase).HasColumnName("NOMIPASE").HasMaxLength(1)
            .HasConversion(FbConverters.SiNo);

        // Columnas NO mapeadas a propósito (decisiones 2026-06-12):
        // - MATRIZ_L / MATRIZ_F: calculadas (SUBSTRING de MATRIZ); usar ValueObjects.LibroMatriz.
        // - BAJAADM / ADMEST / ADMCURSO: módulo administrativo fuera de alcance;
        //   los triggers les dan valor en el INSERT.
        // - UNIVER/AUNIVER/TUNIVER/OUNIVER, ESPEC/AESPEC/TESPEC/OESPEC,
        //   MOROSO/MORO1..3/F_MORO, CD, RESIDE, FFECLIB: columnas muertas
        //   confirmadas por el usuario (todas nullable, no afectan el INSERT).
    }
}
