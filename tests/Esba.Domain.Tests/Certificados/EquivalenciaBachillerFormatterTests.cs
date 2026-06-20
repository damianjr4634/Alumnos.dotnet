using Esba.Domain.Certificados;

namespace Esba.Domain.Tests.Certificados;

public class EquivalenciaBachillerFormatterTests
{
    [Theory]
    [InlineData("BAC", true)]
    [InlineData("BAD", true)]
    [InlineData(" bac ", true)]
    [InlineData("TER", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void EsTipoBachiller_SoloBacYBad(string? tipo, bool esperado) =>
        Assert.Equal(esperado, EquivalenciaBachillerFormatter.EsTipoBachiller(tipo));

    [Theory]
    [InlineData("C", true)]
    [InlineData(" c ", true)]
    [InlineData("A", false)]
    [InlineData(null, false)]
    public void EsTituloEnTramite_SoloC(string? ac, bool esperado) =>
        Assert.Equal(esperado, EquivalenciaBachillerFormatter.EsTituloEnTramite(ac));

    [Theory]
    [InlineData("0000103", "00001/03")] // separa los dos últimos dígitos como año (paridad con el COPY legacy)
    [InlineData("457", "4/57")]
    [InlineData(" 1234 ", "12/34")]
    [InlineData("12", "/12")]
    [InlineData("1", "1")]   // degenerado: sin año, se devuelve tal cual
    [InlineData("", "")]
    [InlineData(null, "")]
    public void FormatearResolucionInterna_SeparaElAnio(string? actint, string esperado) =>
        Assert.Equal(esperado, EquivalenciaBachillerFormatter.FormatearResolucionInterna(actint));

    [Fact]
    public void TextoVista_TituloEnTramite_UsaConstanciaYInstituto()
    {
        var texto = EquivalenciaBachillerFormatter.TextoVista("C", "Instituto Secundario", "Colegio", "Plan 2010");

        Assert.Equal("y teniendo a la vista la (*) constancia de título en trámite otorgado por Instituto Secundario Plan 2010", texto);
    }

    [Fact]
    public void TextoVista_ConAnalitico_UsaCertificadoAnalitico()
    {
        var texto = EquivalenciaBachillerFormatter.TextoVista("A", "Instituto Secundario", "Colegio", "Plan 2010");

        Assert.Equal("y teniendo a la vista el Certificado Analítico del nivel medio otorgado por Instituto Secundario Plan 2010", texto);
    }

    [Fact]
    public void TextoVista_SinInstituto_CaeEnColegio()
    {
        var texto = EquivalenciaBachillerFormatter.TextoVista("A", "  ", "Colegio Nacional", "Plan 2010");

        Assert.EndsWith("otorgado por Colegio Nacional Plan 2010", texto);
    }
}
