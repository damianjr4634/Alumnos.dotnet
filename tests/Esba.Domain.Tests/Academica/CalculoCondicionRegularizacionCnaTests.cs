using Esba.Domain.Academica;

namespace Esba.Domain.Tests.Academica;

public class CalculoCondicionRegularizacionCnaTests
{
    [Theory]
    [InlineData(10, "REGULAR")]
    [InlineData(7, "REGULAR")]     // umbral de aprobación
    [InlineData(6.99, "RECURSA")]
    [InlineData(4, "RECURSA")]
    [InlineData(1, "RECURSA")]     // umbral inferior de recursa
    [InlineData(0.99, "CURSANDO")]
    [InlineData(0, "CURSANDO")]
    public void Resolver_SegunNotaFinal(double nota, string esperada) =>
        Assert.Equal(esperada, CalculoCondicionRegularizacionCna.Resolver((decimal)nota));

    [Fact]
    public void Resolver_SinNota_QuedaCursando() =>
        Assert.Equal("CURSANDO", CalculoCondicionRegularizacionCna.Resolver(null));

    [Fact]
    public void VaAlAnalitico_SoloSiRegular()
    {
        Assert.True(CalculoCondicionRegularizacionCna.VaAlAnalitico(8m));
        Assert.False(CalculoCondicionRegularizacionCna.VaAlAnalitico(5m));
        Assert.False(CalculoCondicionRegularizacionCna.VaAlAnalitico(null));
    }
}
