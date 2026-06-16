using Esba.Domain.Certificados;

namespace Esba.Domain.Tests.Certificados;

public class TextoCastellanoTests
{
    [Theory]
    [InlineData(1, "enero")]
    [InlineData(6, "junio")]
    [InlineData(12, "diciembre")]
    public void MesEnLetras_MesValido_DevuelveNombre(int mes, string esperado)
    {
        Assert.Equal(esperado, TextoCastellano.MesEnLetras(mes));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void MesEnLetras_FueraDeRango_DevuelveVacio(int mes)
    {
        Assert.Equal(string.Empty, TextoCastellano.MesEnLetras(mes));
    }

    [Theory]
    [InlineData(1, "PRIMER")]
    [InlineData(2, "SEGUNDO")]
    [InlineData(3, "TERCER")]
    public void CuatrimestreEnLetras_DevuelveOrdinalEnMayusculas(int cuatrimestre, string esperado)
    {
        Assert.Equal(esperado, TextoCastellano.CuatrimestreEnLetras(cuatrimestre));
    }

    [Fact]
    public void CuatrimestreEnLetras_FueraDeRango_DevuelveNumero()
    {
        Assert.Equal("99", TextoCastellano.CuatrimestreEnLetras(99));
    }
}
