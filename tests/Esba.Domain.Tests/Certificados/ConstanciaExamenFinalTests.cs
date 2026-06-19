using Esba.Domain.Certificados;

namespace Esba.Domain.Tests.Certificados;

public class ConstanciaExamenFinalTests
{
    [Theory]
    [InlineData("* ADEUDA *")]
    [InlineData("CURSANDO")]
    [InlineData("RECURSANDO")]
    [InlineData("EQUIVALENCIA")]
    [InlineData("PREVIA")]
    [InlineData("cursando")]
    [InlineData("  PREVIA  ")]
    public void EsCondicionElegible_CondicionNoRendida_DevuelveFalse(string condicion)
    {
        Assert.False(ConstanciaExamenFinal.EsCondicionElegible(condicion));
    }

    [Theory]
    [InlineData("REGULAR")]
    [InlineData("APROBADA")]
    [InlineData("LIBRE")]
    [InlineData("EXIMIDO")]
    public void EsCondicionElegible_CondicionRendida_DevuelveTrue(string condicion)
    {
        Assert.True(ConstanciaExamenFinal.EsCondicionElegible(condicion));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EsCondicionElegible_SinCondicion_DevuelveTrue(string? condicion)
    {
        // Una condición vacía no está en la lista de no elegibles; el servidor igual
        // falla después al no encontrar párrafo, pero la regla en sí no la bloquea.
        Assert.True(ConstanciaExamenFinal.EsCondicionElegible(condicion));
    }
}
