using Esba.Domain.Academica;

namespace Esba.Domain.Tests.Academica;

public class CalculoCondicionRegularizacionBachilleratoTests
{
    // Por defecto hay carga horaria sin faltas (resul 0 → CONTINASBAC REGULAR).
    private static NotasRegularizacionBachiller Notas(
        decimal? tp = null, decimal? tp2 = null, decimal? recup = null, decimal? regular = null,
        int? totHoras = 100, int? inasist = 0, string condicion = "CURSANDO",
        bool enRecursa = false, string? paso = null) =>
        new(condicion, tp, tp2, recup, regular, totHoras, inasist, enRecursa, paso);

    private static CalculoCondicionRegularizacionBachiller.Resultado Resolver(NotasRegularizacionBachiller n) =>
        CalculoCondicionRegularizacionBachiller.Resolver(n);

    // --- Faltas (_BAC) ---------------------------------------------------------------

    [Fact]
    public void MuchasFaltas_QuedaLibres()
    {
        // resul 50 > 40 → LIBRES sin importar las notas.
        var r = Resolver(Notas(tp: 8m, tp2: 8m, totHoras: 100, inasist: 50));
        Assert.Equal("LIBRES", r.Condicion);
        Assert.False(r.RequiereDecision);
    }

    [Fact]
    public void FaltasEntre26Y40_SinDecision_RequiereDecision()
    {
        var r = Resolver(Notas(tp: 8m, tp2: 8m, totHoras: 100, inasist: 30));
        Assert.True(r.RequiereDecision);
        Assert.Null(r.Condicion);
        Assert.Equal(["Consejo", "Regular", "Libre"], r.Opciones);
    }

    [Fact]
    public void SinCargaHoraria_MantieneLaCondicionDeOrigen()
    {
        // TOT_HORAS 0 → _BAC deja CURSANDO y el ladder mantiene la condición de origen.
        var r = Resolver(Notas(tp: 8m, tp2: 8m, totHoras: 0));
        Assert.Equal("CURSANDO", r.Condicion);
        Assert.Null(r.NotaFinal);
    }

    [Fact]
    public void EnRecursa_ConNotasQueNoPromocionan_RescataARecursando()
    {
        // tpEva2 = 0 mantiene la condición de _BAC; con EnRecursa esa condición es RECURSANDO.
        var r = Resolver(Notas(tp: 2m, tp2: 0m, condicion: "PREVIA", enRecursa: true));
        Assert.Equal("RECURSANDO", r.Condicion);
    }

    [Fact]
    public void SinRecursa_ConNotasQueNoPromocionan_QuedaCursando()
    {
        var r = Resolver(Notas(tp: 2m, tp2: 0m, condicion: "PREVIA", enRecursa: false));
        Assert.Equal("CURSANDO", r.Condicion);
    }

    // --- Ladder de notas (_POSTVAL, pasada inicial) ----------------------------------

    [Fact]
    public void DosBimestresAprobados_QuedaRegular_NotaFinalEsElPromedio()
    {
        var r = Resolver(Notas(tp: 7m, tp2: 8m));
        Assert.Equal("REGULAR", r.Condicion);
        Assert.Equal(7.5m, r.NotaFinal);
        Assert.True(r.VaAlAnalitico);
    }

    [Fact]
    public void AmbosAusentes_QuedaLibres()
    {
        var r = Resolver(Notas(tp: 99m, tp2: 99m));
        Assert.Equal("LIBRES", r.Condicion);
    }

    [Fact]
    public void PromedioBajo_SinRecuperatorio_SinNotaRegular_QuedaAregular()
    {
        // tpEva3 = 2.5 ∈ [1,3.99]; recup 0; regular 0 → A/REGULAR.
        var r = Resolver(Notas(tp: 2m, tp2: 3m, recup: 0m, regular: 0m));
        Assert.Equal("A/REGULAR", r.Condicion);
        Assert.Null(r.NotaFinal);
    }

    [Fact]
    public void PromedioBajo_ConRecuperatorioAprobado_QuedaRegular_NotaFinalEsDefinitiva()
    {
        // tpEva3 = 2.5; recup 8 → REGULAR con nota definitiva (2.5 + 8) / 2 = 5.25.
        var r = Resolver(Notas(tp: 2m, tp2: 3m, recup: 8m));
        Assert.Equal("REGULAR", r.Condicion);
        Assert.Equal(5.25m, r.NotaFinal);
    }

    [Fact]
    public void PromedioBajo_ConNotaRegularAprobada_QuedaRegular_NotaFinalEsLaNotaRegular()
    {
        // tpEva3 = 2.5; recup 0; regular 7 → REGULAR con nota final = REGULAR.
        var r = Resolver(Notas(tp: 2m, tp2: 3m, recup: 0m, regular: 7m));
        Assert.Equal("REGULAR", r.Condicion);
        Assert.Equal(7m, r.NotaFinal);
    }

    [Fact]
    public void PromedioBajo_ConNotaRegularDesaprobada_QuedaPrevio()
    {
        var r = Resolver(Notas(tp: 2m, tp2: 3m, recup: 0m, regular: 4m));
        Assert.Equal("PREVIO", r.Condicion);
        Assert.Null(r.NotaFinal);
    }

    // --- CONSEJO interactivo (PASO) --------------------------------------------------

    [Fact]
    public void Consejo_PasoConsejo_QuedaConsejo()
    {
        var r = Resolver(Notas(tp: 8m, tp2: 8m, inasist: 30, paso: "Consejo"));
        Assert.Equal("CONSEJO", r.Condicion);
        Assert.False(r.RequiereDecision);
    }

    [Fact]
    public void Consejo_PasoLibre_QuedaLibres()
    {
        var r = Resolver(Notas(tp: 8m, tp2: 8m, inasist: 30, paso: "Libre"));
        Assert.Equal("LIBRES", r.Condicion);
    }

    [Fact]
    public void Consejo_PasoRegular_ConNotasAltas_QuedaRegular()
    {
        var r = Resolver(Notas(tp: 7m, tp2: 8m, inasist: 30, paso: "Regular"));
        Assert.Equal("REGULAR", r.Condicion);
        Assert.Equal(7.5m, r.NotaFinal);
    }

    [Fact]
    public void Consejo_PasoRegular_SinNotas_QuedaConsejo()
    {
        // Desde CONSEJO, elegir Regular pero con un bimestre en 0 devuelve CONSEJO (no CURSANDO).
        var r = Resolver(Notas(tp: 8m, tp2: 0m, inasist: 30, paso: "Regular"));
        Assert.Equal("CONSEJO", r.Condicion);
    }

    // --- Derivados -------------------------------------------------------------------

    [Fact]
    public void Promedio_ConAusente_CuentaComoUno()
    {
        // 99 (ausente) computa como 1: promedio = (1 + 8) / 2 = 4.5.
        var r = Resolver(Notas(tp: 99m, tp2: 8m));
        Assert.Equal(4.5m, r.Promedio);
    }
}
