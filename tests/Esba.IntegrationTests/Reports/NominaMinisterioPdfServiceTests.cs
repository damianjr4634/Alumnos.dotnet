using Esba.Application.DTOs.Ministerio;
using Esba.Infrastructure.Reports;
using Microsoft.Extensions.Options;

namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Smoke tests del PDF de la nómina de comisiones al Ministerio: QuestPDF puede
/// arrojar en GeneratePdf ante errores de maqueta (ColumnSpan del subtítulo
/// RECURSANTES, márgenes); verificamos que las variantes (con/sin edad y
/// nacionalidad, con membrete pedido pero sin archivo, margen 0) producen un PDF con
/// una sección con recursantes y otra vacía. No necesitan base de datos.
/// </summary>
public class NominaMinisterioPdfServiceTests
{
    private static readonly byte[] FirmaPdf = [0x25, 0x50, 0x44, 0x46];   // "%PDF"

    private static NominaMinisterioAlumnoDto Alumno(string apellido, int? edad = 20) => new()
    {
        CodigoAlumno = "DNI12345678",
        Apellido = apellido,
        Nombre = "Ana",
        Documento = "DNI 12.345.678",
        Nacionalidad = "ARGENTINA",
        Edad = edad,
    };

    private static NominaMinisterioModel Modelo(bool conEdad, bool conMembrete = false, decimal margen = 4m) => new()
    {
        CarreraLarga = "TECNICATURA SUPERIOR EN GESTIÓN",
        CicloLectivo = 2026,
        FechaInscriptosAl = new DateOnly(2026, 9, 29),
        ConMembrete = conMembrete,
        ConEdadYNacionalidad = conEdad,
        MargenSuperiorCm = margen,
        Secciones =
        [
            new NominaMinisterioSeccion
            {
                Cutuco = 111,
                Cursando = [Alumno("Pérez"), Alumno("Gómez", edad: null)],
                Recursantes = [Alumno("Ruiz")],
            },
            new NominaMinisterioSeccion { Cutuco = 42, Cursando = [], Recursantes = [] },
        ],
    };

    private static NominaMinisterioPdfService CrearServicio(string? membrete = null, string? sello = null) => new(
        Options.Create(new InstitucionSettings
        {
            Nombre = "Instituto de Estudios Superiores de Buenos Aires",
            MembreteConstanciaPath = membrete,
            SelloPath = sello,
        }));

    [Fact]
    public void Nomina_ConMembreteYSello_IncrustaLasImagenesEnLaUltimaHoja()
    {
        using var imagenes = ImagenesDePrueba.Crear();

        var pdf = CrearServicio(membrete: imagenes.Membrete, sello: imagenes.Sello)
            .GenerarNomina(Modelo(conEdad: false, conMembrete: true));

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }

    [Fact]
    public void Nomina_SinEdad_ProduceUnPdf()
    {
        var pdf = CrearServicio().GenerarNomina(Modelo(conEdad: false));

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }

    [Fact]
    public void Nomina_ConEdadYNacionalidad_ProduceUnPdf()
    {
        var pdf = CrearServicio().GenerarNomina(Modelo(conEdad: true));

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }

    [Fact]
    public void Nomina_ConMembretePedidoPeroSinArchivo_ImprimeSinFondo()
    {
        var pdf = CrearServicio(membrete: "no-existe/membrete.jpg")
            .GenerarNomina(Modelo(conEdad: false, conMembrete: true, margen: 0m));

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }
}
