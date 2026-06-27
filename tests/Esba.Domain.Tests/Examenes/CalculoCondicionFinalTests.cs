using Esba.Domain.Examenes;
using Xunit;

namespace Esba.Domain.Tests.Examenes;

/// <summary>
/// Cubre la lógica portada de FinalesxMesayComision.GraboNotaASC/GraboNotaBAC y
/// la selección de nota para el analítico de XXX_MESAS.
/// </summary>
public class CalculoCondicionFinalTests
{
    private static NotaFinal N(decimal? nota) => new(nota, nota is null ? null : new DateOnly(2026, 6, 1), nota is null ? null : "L1");

    // --- Terciaria ---

    [Fact]
    public void Terciaria_TresAplazos_Recursa_SinAnalitico()
    {
        var r = CalculoCondicionFinal.Terciaria(N(2), N(3), N(1), "REGULAR");

        Assert.Equal("RECURSA", r.Condicion);
        Assert.False(r.AprobóAlAnalitico);
    }

    [Fact]
    public void Terciaria_PrimeraAprueba_Final_ConAnaliticoDeLaPrimera()
    {
        var r = CalculoCondicionFinal.Terciaria(N(7), N(null), N(null), "REGULAR");

        Assert.Equal("FINAL", r.Condicion);
        Assert.True(r.AprobóAlAnalitico);
        Assert.Equal(7m, r.NotaAnalitico);
    }

    [Fact]
    public void Terciaria_AplazaPrimeraApruebaSegunda_AnaliticoDeLaSegunda()
    {
        var r = CalculoCondicionFinal.Terciaria(N(2), N(8), N(null), "REGULAR");

        Assert.Equal("FINAL", r.Condicion);
        Assert.Equal(8m, r.NotaAnalitico);
    }

    [Fact]
    public void Terciaria_SinAprobarYSinTresAplazos_ConservaAnterior_SinAnalitico()
    {
        var r = CalculoCondicionFinal.Terciaria(N(3), N(null), N(null), "REGULAR");

        Assert.Equal("REGULAR", r.Condicion);
        Assert.False(r.AprobóAlAnalitico);
    }

    [Fact]
    public void Terciaria_TodoVacio_ConservaAnterior()
    {
        var r = CalculoCondicionFinal.Terciaria(N(null), N(null), N(null), "PREVIA");

        Assert.Equal("PREVIA", r.Condicion);
        Assert.False(r.AprobóAlAnalitico);
    }

    // --- Bachiller ---

    [Theory]
    [InlineData("PREVIO", "LIBRE")]
    [InlineData("PREVIA", "LIBRE")]
    [InlineData("LIBRES", "LIBRE")]
    [InlineData("DICIEMBRE", "LIBRE")]
    [InlineData("MARZO", "LIBRE")]
    [InlineData("LIBRE", "LIBRE")]
    [InlineData("P/EQUIVALEN", "FINAL")]
    public void Bachiller_NotaAprobada_MapeaCondicion(string anterior, string esperada)
    {
        var r = CalculoCondicionFinal.Bachiller(N(8), anterior);

        Assert.Equal(esperada, r.Condicion);
        Assert.True(r.AprobóAlAnalitico);
        Assert.Equal(8m, r.NotaAnalitico);
    }

    [Fact]
    public void Bachiller_NotaDesaprobada_ConservaAnterior_SinAnalitico()
    {
        var r = CalculoCondicionFinal.Bachiller(N(5), "PREVIO");

        Assert.Equal("PREVIO", r.Condicion);
        Assert.False(r.AprobóAlAnalitico);
    }

    [Fact]
    public void Bachiller_CondicionNoListada_ConservaAnterior_SinAnalitico()
    {
        // 'CURSANDO' no está en el mapa: queda como estaba y no va al analítico.
        var r = CalculoCondicionFinal.Bachiller(N(9), "CURSANDO");

        Assert.Equal("CURSANDO", r.Condicion);
        Assert.False(r.AprobóAlAnalitico);
    }
}
