using Esba.Application.DTOs.Certificados;
using Esba.Domain.Certificados;
using Esba.Infrastructure.Reports;
using Microsoft.Extensions.Options;

namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Smoke tests del reporte tabular "Constancia de Materias Aprobadas": QuestPDF puede
/// arrojar en <c>GeneratePdf</c> ante errores de maqueta (p.ej. column spans mal
/// formados), así que verificamos que produce un PDF no vacío con filas mixtas. No
/// necesita base de datos (sin el trait Integration: corre en el ciclo rápido).
/// </summary>
public class ConstanciaAnaliticoPdfServiceTests
{
    private static ConstanciaAnaliticoPdfService Crear() =>
        new(Options.Create(new InstitucionSettings { Nombre = "ESBA", Caracteristica = "A-781" }));

    private static ConstanciaAnaliticoPdfService CrearServicioConImagenes(ImagenesDePrueba imagenes) =>
        new(Options.Create(new InstitucionSettings
        {
            Nombre = "ESBA",
            Caracteristica = "A-781",
            MembreteConstanciaPath = imagenes.Membrete,
            SelloPath = imagenes.Sello,
            FirmaRectorPath = imagenes.Firma,
            FirmaSecretariaPath = imagenes.Firma,
        }));

    private static ConstanciaMateriasAprobadasModel Modelo() => new()
    {
        Introduccion = "En Buenos Aires a los 18 días del mes de junio de 2026 …",
        AnteQuien = "Quien corresponda",
        Secretaria = "Secretaria Test",
        Rector = "Rectora Test",
        Filas =
        [
            new FilaAnaliticoConstancia { Cuatrimestre = 1, Materia = "Análisis", Condicion = "APROBADA", Nota = "9.00", Fecha = "01/03/2024", Instituto = "ESBA A-781" },
            new FilaAnaliticoConstancia { Cuatrimestre = 1, Materia = "Lógica", Condicion = "ADEUDA", Nota = "—", Fecha = "—", Instituto = "—" },
            new FilaAnaliticoConstancia { Cuatrimestre = 2, Materia = "Idioma", Condicion = "MATERIA ANUAL", OcupaFilaCompleta = true },
            new FilaAnaliticoConstancia { Cuatrimestre = 2, Materia = "Historia", Condicion = "APROBADA POR EQUIVALENCIA - Act. Interna N° 12", OcupaFilaCompleta = true },
        ],
    };

    [Fact]
    public void GenerarMateriasAprobadas_ConFilasMixtas_ProduceUnPdf()
    {
        var pdf = Crear().GenerarMateriasAprobadas(Modelo());

        Assert.NotEmpty(pdf);
        // Firma de archivo PDF ("%PDF").
        Assert.Equal(new byte[] { 0x25, 0x50, 0x44, 0x46 }, pdf[..4]);
    }

    [Fact]
    public void GenerarMateriasAprobadas_ConMembreteSelloYFirmas_IncrustaLasImagenes()
    {
        using var imagenes = ImagenesDePrueba.Crear();

        var pdf = CrearServicioConImagenes(imagenes).GenerarMateriasAprobadas(Modelo());

        Assert.NotEmpty(pdf);
        Assert.Equal(new byte[] { 0x25, 0x50, 0x44, 0x46 }, pdf[..4]);
        Assert.True(pdf.Length > Crear().GenerarMateriasAprobadas(Modelo()).Length);
    }

    [Fact]
    public void GenerarMateriasAprobadas_SinFirmantes_NoIncrustaFirmas()
    {
        using var imagenes = ImagenesDePrueba.Crear();
        var sinNombres = Modelo() with { Secretaria = null, Rector = null };

        // Las imágenes de firma solo acompañan a un nombre impreso: sin nombres, el PDF
        // lleva membrete y sello pero ninguna firma, y pesa menos que con ambas.
        var sinFirmas = CrearServicioConImagenes(imagenes).GenerarMateriasAprobadas(sinNombres);
        var conFirmas = CrearServicioConImagenes(imagenes).GenerarMateriasAprobadas(Modelo());

        Assert.Equal(new byte[] { 0x25, 0x50, 0x44, 0x46 }, sinFirmas[..4]);
        Assert.True(conFirmas.Length > sinFirmas.Length);
    }

    [Fact]
    public void GenerarMateriasAprobadas_SinFilas_ProduceUnPdf()
    {
        var modelo = Modelo() with { Filas = [] };

        var pdf = Crear().GenerarMateriasAprobadas(modelo);

        Assert.NotEmpty(pdf);
    }
}
