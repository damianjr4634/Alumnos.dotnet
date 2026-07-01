using Esba.Domain.Academica;

namespace Esba.Domain.Tests.Academica;

public class CalculoCondicionRegularizacionTerciariaTests
{
    // Por defecto hay carga horaria sin faltas (resul 0): el SP legacy fuerza CURSANDO
    // cuando TOT_HORAS = 0, así que las notas solo importan con horas > 0.
    private static NotasRegularizacionTerciaria Notas(
        decimal? tp = null, decimal? tp2 = null, decimal? recup = null,
        int? totHoras = 100, int? inasist = 0, int? justif = 0,
        string condicion = "CURSANDO", bool promociona = false, bool apruebaSinFinal = false) =>
        new(condicion, tp, tp2, recup, totHoras, inasist, justif, promociona, apruebaSinFinal);

    private static string Condicion(NotasRegularizacionTerciaria n, decimal notaPromocion = 7m) =>
        CalculoCondicionRegularizacionTerciaria.ResolverCondicion(n, notaPromocion);

    [Fact]
    public void SinNotas_MantieneLaCondicionActual() =>
        Assert.Equal("CURSANDO", Condicion(Notas(condicion: "CURSANDO")));

    [Fact]
    public void UnSoloParcial_MantieneLaCondicionActual() =>
        Assert.Equal("CURSANDO", Condicion(Notas(tp: 8m)));

    [Fact]
    public void DosParcialesAprobados_QuedaRegular() =>
        Assert.Equal("REGULAR", Condicion(Notas(tp: 7m, tp2: 6m)));

    [Fact]
    public void ParcialDesaprobado_ConRecuperatorioAprobado_QuedaRegular() =>
        Assert.Equal("REGULAR", Condicion(Notas(tp: 3m, tp2: 5m, recup: 7m)));

    [Fact]
    public void ParcialDesaprobado_ConRecuperatorioDesaprobado_Recursa() =>
        Assert.Equal("RECURSA", Condicion(Notas(tp: 2m, tp2: 3m, recup: 3m)));

    [Fact]
    public void Ausente99_SinRecuperatorio_MantieneCondicion() =>
        Assert.Equal("CURSANDO", Condicion(Notas(tp: 99m, tp2: 5m, recup: 0m)));

    [Fact]
    public void Ausente99_ConRecuperatorio99_Recursa() =>
        Assert.Equal("RECURSA", Condicion(Notas(tp: 99m, tp2: 99m, recup: 99m)));

    [Fact]
    public void ReincorporaSinNotas_CaeAFallbackCursando() =>
        Assert.Equal("CURSANDO", Condicion(Notas(condicion: "REINCORPORA")));

    [Fact]
    public void MuchasInasistencias_PasaALibre() =>
        // TOT_HORAS 100, INASIST 70 -> resul 70 > 60 -> LIBRE (aun con parciales aprobados).
        Assert.Equal("LIBRE", Condicion(Notas(tp: 8m, tp2: 8m, totHoras: 100, inasist: 70, justif: 0)));

    [Fact]
    public void FaltasModeradasConInasistenciaAlta_Reincorpora() =>
        // resul 45 (26..50), resulInasist 45 > 25 -> REINCORPORA.
        Assert.Equal("REINCORPORA", Condicion(Notas(tp: 8m, tp2: 8m, totHoras: 100, inasist: 45, justif: 0)));

    [Fact]
    public void RegularConMateriaPromocionYNotasAltas_Promociona() =>
        Assert.Equal("PROMOCIONA", Condicion(Notas(tp: 8m, tp2: 9m, totHoras: 100, inasist: 0, justif: 0, promociona: true), notaPromocion: 7m));

    [Fact]
    public void RegularConMateriaPromocionPeroNotaBaja_QuedaRegular() =>
        Assert.Equal("REGULAR", Condicion(Notas(tp: 6m, tp2: 9m, totHoras: 100, inasist: 0, justif: 0, promociona: true), notaPromocion: 7m));

    [Fact]
    public void RegularConMateriaApruebaSinFinal_QuedaFinal() =>
        Assert.Equal("FINAL", Condicion(Notas(tp: 6m, tp2: 6m, totHoras: 100, inasist: 0, justif: 0, apruebaSinFinal: true)));

    [Fact]
    public void Resolver_Promociona_NotaAnaliticoEsPromedio()
    {
        var r = CalculoCondicionRegularizacionTerciaria.Resolver(
            Notas(tp: 8m, tp2: 9m, totHoras: 100, inasist: 0, justif: 0, promociona: true), 7m);

        Assert.Equal("PROMOCIONA", r.Condicion);
        Assert.True(r.VaAlAnalitico);
        Assert.Equal(8.5m, r.NotaAnalitico);
    }

    [Fact]
    public void Resolver_FinalConParcialBajo_NotaAnaliticoEsRecuperatorio()
    {
        var r = CalculoCondicionRegularizacionTerciaria.Resolver(
            Notas(tp: 3m, tp2: 6m, recup: 7m, totHoras: 100, inasist: 0, justif: 0, apruebaSinFinal: true), 7m);

        Assert.Equal("FINAL", r.Condicion);
        Assert.Equal(7m, r.NotaAnalitico);
    }

    [Fact]
    public void Resolver_Regular_NoVaAlAnalitico()
    {
        var r = CalculoCondicionRegularizacionTerciaria.Resolver(Notas(tp: 7m, tp2: 6m), 7m);

        Assert.Equal("REGULAR", r.Condicion);
        Assert.False(r.VaAlAnalitico);
        Assert.Null(r.NotaAnalitico);
    }
}
