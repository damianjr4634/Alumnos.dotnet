using Esba.Domain.Academica;

namespace Esba.Domain.Tests.Academica;

/// <summary>
/// La precarga del docente pide exactamente los campos que secretaría edita en la
/// regularización de cada variante (hito 15): la variante se decide igual que en la
/// pantalla de regularización por comisión.
/// </summary>
public class EsquemaCargaCursadoTests
{
    [Theory]
    [InlineData("ASC19", "TER", VarianteRegularizacion.Terciaria)]
    [InlineData("226518", "TER", VarianteRegularizacion.Terciaria)]
    [InlineData("BAC", "BAD", VarianteRegularizacion.Bachillerato)]
    [InlineData("bac", "BAD", VarianteRegularizacion.Bachillerato)]
    [InlineData("333", "BAC", VarianteRegularizacion.Secundario)]
    [InlineData("650", "BAC", VarianteRegularizacion.Secundario)]
    [InlineData("CNA", "BAC", VarianteRegularizacion.Cna)]
    [InlineData("197916", "BAD", VarianteRegularizacion.NoDisponible)]
    [InlineData("XYZ", null, VarianteRegularizacion.NoDisponible)]
    public void DeterminarVariante_MismaDecisionQueSecretaria(string carrera, string? tipo, VarianteRegularizacion esperada) =>
        Assert.Equal(esperada, EsquemaCargaCursado.DeterminarVariante(carrera, tipo));

    [Fact]
    public void Terciaria_DosParcialesUnRecuperatorioHorasInasistenciasJustificadas()
    {
        var e = EsquemaCargaCursado.Para(VarianteRegularizacion.Terciaria);

        Assert.True(e.Disponible);
        Assert.True(e.Evaluacion1 && e.Evaluacion2 && e.Recuperatorio && e.Horas && e.Inasistencias && e.Justificadas);
        Assert.False(e.Evaluacion3 || e.NotaRegular || e.NotaFinal);
        Assert.Equal("1° Parc.", e.EtiquetaEvaluacion1);
    }

    [Fact]
    public void Bachillerato_AgregaNotaARegularizarYEtiquetaBimestres()
    {
        var e = EsquemaCargaCursado.Para(VarianteRegularizacion.Bachillerato);

        Assert.True(e.Evaluacion1 && e.Evaluacion2 && e.Recuperatorio && e.NotaRegular && e.Horas && e.Inasistencias && e.Justificadas);
        Assert.False(e.Evaluacion3 || e.NotaFinal);
        Assert.Equal("1° Bim.", e.EtiquetaEvaluacion1);
    }

    [Fact]
    public void Secundario_TresTrimestresSinRecuperatorioNiJustificadas()
    {
        var e = EsquemaCargaCursado.Para(VarianteRegularizacion.Secundario);

        Assert.True(e.Evaluacion1 && e.Evaluacion2 && e.Evaluacion3 && e.Horas && e.Inasistencias);
        Assert.False(e.Recuperatorio || e.NotaRegular || e.NotaFinal || e.Justificadas);
        Assert.Equal("3° Trim.", e.EtiquetaEvaluacion3);
    }

    [Fact]
    public void Cna_SoloNotaFinal()
    {
        var e = EsquemaCargaCursado.Para(VarianteRegularizacion.Cna);

        Assert.True(e.NotaFinal);
        Assert.False(e.Evaluacion1 || e.Evaluacion2 || e.Evaluacion3 || e.Recuperatorio || e.NotaRegular
            || e.Horas || e.Inasistencias || e.Justificadas);
    }

    [Fact]
    public void NoDisponible_NoMuestraNada()
    {
        var e = EsquemaCargaCursado.Para("197916", "BAD");

        Assert.False(e.Disponible);
        Assert.False(e.Evaluacion1 || e.Evaluacion2 || e.Evaluacion3 || e.Recuperatorio || e.NotaRegular
            || e.NotaFinal || e.Horas || e.Inasistencias || e.Justificadas);
    }
}
