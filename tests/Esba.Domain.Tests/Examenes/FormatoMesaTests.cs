using Esba.Domain.Examenes;

namespace Esba.Domain.Tests.Examenes;

/// <summary>Formatos de las impresiones de mesas (sucesores de las expresiones SQL de Impresiones.pas).</summary>
public class FormatoMesaTests
{
    [Theory]
    [InlineData(930, "9:30")]
    [InlineData(1830, "18:30")]
    [InlineData(800, "8:00")]
    [InlineData(1905, "19:05")]
    [InlineData(0, "")]
    [InlineData(null, "")]
    public void Hora_SeparaHorasYMinutosDelNumeroHHMM(int? hora, string esperado)
    {
        Assert.Equal(esperado, FormatoMesa.Hora(hora));
    }

    [Theory]
    [InlineData(111, 112, 113, "111/112/113")]
    [InlineData(111, null, null, "111/0/0")]
    [InlineData(null, null, null, "0/0/0")]
    public void Comisiones_TresComisionesConCeroParaNulos(int? c1, int? c2, int? c3, string esperado)
    {
        Assert.Equal(esperado, FormatoMesa.Comisiones(c1, c2, c3));
    }

    [Theory]
    [InlineData("PÉREZ, ANA", "GÓMEZ, LUIS", "RUIZ, EVA", "PÉREZ, ANA - GÓMEZ, LUIS - RUIZ, EVA")]
    [InlineData("PÉREZ, ANA", null, " RUIZ, EVA ", "PÉREZ, ANA - RUIZ, EVA")]
    [InlineData(null, "", null, "")]
    public void Docentes_UneSoloLosPresentes(string? titular, string? vocal1, string? vocal2, string esperado)
    {
        Assert.Equal(esperado, FormatoMesa.Docentes(titular, vocal1, vocal2));
    }

    [Theory]
    [InlineData(FirmanteCitacion.Rector, "RECTOR/A")]
    [InlineData(FirmanteCitacion.Secretaria, "SECRETARIA")]
    [InlineData(FirmanteCitacion.DirectorEstudios, "DIR. DE ESTUDIOS")]
    public void Firmante_CargoComoElLegacy(FirmanteCitacion firmante, string esperado)
    {
        Assert.Equal(esperado, firmante.Cargo());
    }
}
