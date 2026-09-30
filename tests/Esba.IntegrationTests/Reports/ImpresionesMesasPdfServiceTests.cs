using Esba.Application.DTOs.Examenes;
using Esba.Domain.Examenes;
using Esba.Infrastructure.Reports;
using Microsoft.Extensions.Options;

namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Smoke tests de los PDF de impresiones de mesas: QuestPDF puede arrojar en
/// GeneratePdf ante errores de maqueta (ColumnSpan del subtítulo por carrera, bloque de
/// firma con ancho fijo); verificamos que la citación (con y sin imagen de firma, con
/// un docente sin mesas) y el parte diario (dos fechas, dos carreras) producen un PDF.
/// No necesitan base de datos.
/// </summary>
public class ImpresionesMesasPdfServiceTests
{
    private static readonly byte[] FirmaPdf = [0x25, 0x50, 0x44, 0x46];   // "%PDF"
    private static readonly DateOnly Fecha = new(2026, 12, 10);

    private static IOptions<InstitucionSettings> Settings(string? firmaRector = null) => Options.Create(new InstitucionSettings
    {
        Nombre = "Instituto de Estudios Superiores de Buenos Aires",
        Caracteristica = "A-781",
        MembreteConstanciaPath = "no-existe/membrete.jpg",
        FirmaRectorPath = firmaRector,
    });

    private static CitacionDocentesModel Citacion(bool conImagen) => new()
    {
        FechaEmision = new DateOnly(2026, 9, 29),
        Firmante = FirmanteCitacion.Rector,
        NombreFirmante = "RECTORA UNO",
        ConImagenFirma = conImagen,
        Docentes =
        [
            new CitacionDocenteSeccion
            {
                CodigoProfesor = "001",
                Docente = "PÉREZ, ANA",
                Mesas =
                [
                    new MesaCitacionDto { CodigoProfesor = "001", FechaExamen = Fecha, Hora = 930, Materia = "CONTABILIDAD I", Mesa = 12, Aula = 3, CodigoCarrera = "TEC" },
                    new MesaCitacionDto { CodigoProfesor = "001", FechaExamen = Fecha.AddDays(2), Hora = 1830, Materia = "PSICOLOGÍA", Mesa = 15, Aula = null, CodigoCarrera = "BAC" },
                ],
            },
            new CitacionDocenteSeccion { CodigoProfesor = "002", Docente = "GÓMEZ, LUIS", Mesas = [] },
        ],
    };

    private static ParteDiarioMesasModel ParteDiario() => new()
    {
        CicloLectivo = 2026,
        Dias =
        [
            new ParteDiarioDia
            {
                Fecha = Fecha,
                Carreras =
                [
                    new ParteDiarioCarrera
                    {
                        CodigoCarrera = "BAC",
                        NombreCarrera = "BACHILLERATO",
                        Mesas =
                        [
                            new ParteDiarioMesaDto { FechaExamen = Fecha, CodigoCarrera = "BAC", Mesa = 1, Hora = 830, Materia = "MAT", Titular = "PÉREZ, ANA", Vocal1 = "GÓMEZ, LUIS", Comision1 = 111, Comision2 = 112, CantidadAlumnos = 12, Aula = 4 },
                            new ParteDiarioMesaDto { FechaExamen = Fecha, CodigoCarrera = "BAC", Mesa = 2, Hora = 1830, Materia = "LENGUA", CantidadAlumnos = 0 },
                        ],
                    },
                    new ParteDiarioCarrera
                    {
                        CodigoCarrera = "TEC",
                        NombreCarrera = null,
                        Mesas = [new ParteDiarioMesaDto { FechaExamen = Fecha, CodigoCarrera = "TEC", Mesa = 7, Hora = 1900, Materia = "CONTABILIDAD I", Titular = "RUIZ, EVA", CantidadAlumnos = 3, Aula = 1 }],
                    },
                ],
            },
            new ParteDiarioDia { Fecha = Fecha.AddDays(1), Carreras = [] },
        ],
    };

    [Fact]
    public void Citacion_SinImagenDeFirma_ProduceUnPdf()
    {
        var pdf = new CitacionDocentesPdfService(Settings()).GenerarCitacion(Citacion(conImagen: false));

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }

    [Fact]
    public void Citacion_ConImagenPedidaPeroSinArchivo_ImprimeSoloNombreYCargo()
    {
        var pdf = new CitacionDocentesPdfService(Settings(firmaRector: "no-existe/firma.jpg"))
            .GenerarCitacion(Citacion(conImagen: true));

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }

    [Fact]
    public void ParteDiario_ProduceUnPdf()
    {
        var pdf = new ParteDiarioMesasPdfService().GenerarParteDiario(ParteDiario());

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }
}
