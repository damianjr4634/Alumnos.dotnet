using Esba.Application.DTOs.Asistencias;
using Esba.Domain.Asistencias;
using Esba.Infrastructure.Reports;
using Microsoft.Extensions.Options;

namespace Esba.IntegrationTests.Reports;

/// <summary>
/// Smoke tests del PDF de las carpetas por comisión: QuestPDF puede arrojar en
/// GeneratePdf ante errores de maqueta (ambas grillas usan RowSpan/ColumnSpan);
/// verificamos que los dos layouts producen un PDF con datos realistas, incluidas
/// una sección con recursantes y otra vacía. No necesitan base de datos (sin el
/// trait Integration).
/// </summary>
public class CarpetaComisionPdfServiceTests
{
    private static CarpetaComisionModel Modelo(TipoCarpetaComision tipo) => new()
    {
        Tipo = tipo,
        CarreraLarga = "Bachillerato de Adultos",
        CuatrimestreAnio = "1/24",
        FechaEmision = new DateOnly(2026, 7, 14),
        Secciones =
        [
            new CarpetaComisionSeccion
            {
                Cabecera = new CarpetaComisionCabeceraDto
                {
                    Cutuco = 111, CodigoMateria = "01", DescripcionMateria = "Contabilidad",
                    Docente = "RODRIGUEZ, LUIS", TitularSuplente = "T",
                },
                Cursando =
                [
                    new CarpetaComisionAlumnoDto { CodigoAlumno = "100", Apellido = "Pérez", Nombre = "Ana", Condicion = "CURSANDO", Cutuco = 111, CodigoMateria = "01" },
                    new CarpetaComisionAlumnoDto { CodigoAlumno = "101", Apellido = "Gómez", Nombre = "Luis", Condicion = "CURSANDO", Cutuco = 111, CodigoMateria = "01" },
                ],
                Recursantes =
                [
                    new CarpetaComisionAlumnoDto { CodigoAlumno = "102", Apellido = "Ruiz", Nombre = "Eva", Condicion = "RECURSANDO", Cutuco = 111, CodigoMateria = "01" },
                ],
            },
            new CarpetaComisionSeccion
            {
                Cabecera = new CarpetaComisionCabeceraDto
                {
                    Cutuco = 222, CodigoMateria = "02", DescripcionMateria = "Psicología",
                    Docente = null, TitularSuplente = null,
                },
                Cursando = [],
                Recursantes = [],
            },
        ],
    };

    private static readonly byte[] FirmaPdf = [0x25, 0x50, 0x44, 0x46];   // "%PDF"

    private static CarpetaComisionPdfService CrearServicio() => new(
        Options.Create(new InstitucionSettings { Nombre = "Instituto de Estudios Superiores de Buenos Aires" }));

    [Fact]
    public void CarpetaAsistencia_Pdf_ProduceUnPdf()
    {
        var pdf = CrearServicio().GenerarCarpeta(Modelo(TipoCarpetaComision.Asistencia));

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }

    [Fact]
    public void CarpetaTrabajosPracticos_Pdf_ProduceUnPdf()
    {
        var pdf = CrearServicio().GenerarCarpeta(Modelo(TipoCarpetaComision.TrabajosPracticos));

        Assert.NotEmpty(pdf);
        Assert.Equal(FirmaPdf, pdf[..4]);
    }
}
