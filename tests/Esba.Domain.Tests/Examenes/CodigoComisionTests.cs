using Esba.Domain.Examenes;

namespace Esba.Domain.Tests.Examenes;

public class CodigoComisionTests
{
    [Theory]
    [InlineData(111, 1, 1, 1)]
    [InlineData(234, 2, 3, 4)]
    [InlineData(123, 1, 2, 3)]
    [InlineData(646, 6, 4, 6)]
    public void TryDescomponer_TresDigitos_SeparaCuatrimestreTurnoComision(
        int cutuco, int cuatrimestre, int turno, int comision)
    {
        var ok = CodigoComision.TryDescomponer(cutuco, out var codigo);

        Assert.True(ok);
        Assert.Equal(cuatrimestre, codigo.Cuatrimestre);
        Assert.Equal(turno, codigo.Turno);
        Assert.Equal(comision, codigo.Comision);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(99)]
    public void TryDescomponer_MenosDeTresDigitos_NoEsDescomponible(int cutuco)
    {
        // El legacy solo decodifica cuando CUTUCO >= 100 (lstactasMesas.pas).
        var ok = CodigoComision.TryDescomponer(cutuco, out _);

        Assert.False(ok);
    }

    [Theory]
    [InlineData(1, "Mañana")]
    [InlineData(2, "Tarde")]
    [InlineData(3, "Vespertino")]
    [InlineData(4, "Noche")]
    public void TurnoEnLetras_ValoresConocidos_DevuelveEtiqueta(int turno, string esperado) =>
        Assert.Equal(esperado, CodigoComision.TurnoEnLetras(turno));

    [Fact]
    public void TurnoEnLetras_ValorDesconocido_DevuelveElNumero() =>
        Assert.Equal("7", CodigoComision.TurnoEnLetras(7));

    [Theory]
    [InlineData(1, "A")]
    [InlineData(2, "B")]
    [InlineData(5, "E")]
    public void ComisionEnLetras_DevuelveLetra(int comision, string esperado) =>
        Assert.Equal(esperado, CodigoComision.ComisionEnLetras(comision));

    [Fact]
    public void Propiedades_DevuelvenTextoDeLaInstancia()
    {
        CodigoComision.TryDescomponer(232, out var codigo);

        Assert.Equal("Vespertino", codigo.TurnoTexto);
        Assert.Equal("B", codigo.ComisionTexto);
    }
}
