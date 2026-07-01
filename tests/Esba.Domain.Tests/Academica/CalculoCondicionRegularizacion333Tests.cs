using Esba.Domain.Academica;

namespace Esba.Domain.Tests.Academica;

public class CalculoCondicionRegularizacion333Tests
{
    private static readonly DateTime FecEva2 = new(2024, 7, 1);
    private static readonly DateTime FechDic = new(2024, 12, 15);
    private static readonly DateTime FechMar = new(2025, 3, 10);

    private static NotasRegularizacion333 Notas(
        decimal? tp = null, decimal? tp2 = null, decimal? dic = null, decimal? mar = null,
        DateTime? fecEva2 = null, DateTime? fechDic = null, DateTime? fechMar = null,
        string condicion = "CURSANDO") =>
        new(condicion, tp, tp2, dic, mar, fecEva2, fechDic, fechMar);

    [Fact]
    public void SegundoTrimestreAprobado_QuedaRegular_ConNotaYFechaDelTrimestre()
    {
        var r = CalculoCondicionRegularizacion333.Resolver(Notas(tp: 6m, tp2: 7m, fecEva2: FecEva2));

        Assert.Equal("REGULAR", r.Condicion);
        Assert.Equal(7m, r.NotaFinal);
        Assert.Equal(FecEva2, r.NotaFinalFecha);
        Assert.True(r.VaAlAnalitico);
    }

    [Fact]
    public void SegundoTrimestreDesaprobado_SinDiciembreNiMarzo_QuedaEnProceso()
    {
        var r = CalculoCondicionRegularizacion333.Resolver(Notas(tp: 5m, tp2: 4m, dic: 0m, mar: 0m));

        Assert.Equal("ENPROCESO", r.Condicion);
        Assert.Equal(0m, r.NotaFinal);
        Assert.Null(r.NotaFinalFecha);
        Assert.False(r.VaAlAnalitico);
    }

    [Fact]
    public void DiciembreAprueba_QuedaRegular_ConNotaYFechaDeDiciembre()
    {
        var r = CalculoCondicionRegularizacion333.Resolver(Notas(tp: 5m, tp2: 4m, dic: 8m, fechDic: FechDic));

        Assert.Equal("REGULAR", r.Condicion);
        Assert.Equal(8m, r.NotaFinal);
        Assert.Equal(FechDic, r.NotaFinalFecha);
    }

    [Fact]
    public void MarzoAprueba_CuandoDiciembreNoAlcanza_QuedaRegular()
    {
        var r = CalculoCondicionRegularizacion333.Resolver(Notas(tp: 5m, tp2: 4m, dic: 0m, mar: 7m, fechMar: FechMar));

        Assert.Equal("REGULAR", r.Condicion);
        Assert.Equal(7m, r.NotaFinal);
        Assert.Equal(FechMar, r.NotaFinalFecha);
    }

    [Fact]
    public void DiciembreAplazado_QuedaPrevia()
    {
        var r = CalculoCondicionRegularizacion333.Resolver(Notas(tp: 5m, tp2: 4m, dic: 3m));

        Assert.Equal("PREVIA", r.Condicion);
        Assert.False(r.VaAlAnalitico);
    }

    [Fact]
    public void SegundoTrimestreSinCargar_MantieneLaCondicionDeOrigen()
    {
        var r = CalculoCondicionRegularizacion333.Resolver(Notas(tp: 5m, tp2: 0m, condicion: "CURSANDO"));

        Assert.Equal("CURSANDO", r.Condicion);
    }

    [Fact]
    public void AmbosTrimestresAusentes_SinExamenes_QuedaEnProceso()
    {
        var r = CalculoCondicionRegularizacion333.Resolver(Notas(tp: 99m, tp2: 99m, dic: 0m, mar: 0m));

        Assert.Equal("ENPROCESO", r.Condicion);
    }

    [Fact]
    public void DiciembreApruebaSinFecha_MarcaFaltaFecha()
    {
        var r = CalculoCondicionRegularizacion333.Resolver(Notas(tp: 5m, tp2: 4m, dic: 8m, fechDic: null));

        Assert.Equal("REGULAR", r.Condicion);
        Assert.True(r.FaltaFecha);
    }
}
