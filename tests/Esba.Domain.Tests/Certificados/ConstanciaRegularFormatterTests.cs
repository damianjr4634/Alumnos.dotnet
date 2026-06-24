using Esba.Domain.Certificados;

namespace Esba.Domain.Tests.Certificados;

public class ConstanciaRegularFormatterTests
{
    private static ConstanciaRegularContexto Contexto(
        bool aDistancia = false, bool porAnio = false, int cuatrimestre = 1, int turno = 1,
        string codigoCarrera = "ABC", string? dictamen = null) => new()
    {
        NombreCompleto = "Pérez, Juan",
        CodigoConPuntos = "12.345",
        NombreCarrera = "Tecnicatura en Programación",
        CodigoCarrera = codigoCarrera,
        EsADistancia = aDistancia,
        Cuatrimestre = cuatrimestre,
        Turno = turno,
        EsCarreraPorAnio = porAnio,
        Dictamen = dictamen,
        AnteQuien = "quien corresponda",
        Fecha = new DateOnly(2026, 6, 23),
    };

    [Fact]
    public void Cuerpo_Presencial_LlevaCarreraHorarioYCuatrimestre()
    {
        var cuerpo = ConstanciaRegularFormatter.Cuerpo(Contexto(cuatrimestre: 1, turno: 1, codigoCarrera: "ABC"));

        var texto = string.Join("\n", cuerpo);
        Assert.Contains("es alumno regular de la Carrera TECNICATURA EN PROGRAMACIÓN - primer cuatrimestre.", texto);
        Assert.Contains("Asiste los dias: Lunes a Viernes en el horario de 8:30 a 11:45 hs.", texto);
        Assert.Contains("presentada ante  QUIEN CORRESPONDA.", texto);
        Assert.Contains("A los  23 días del mes de junio de 2026.", texto);
        Assert.DoesNotContain("Dictamen", texto);
    }

    [Fact]
    public void Cuerpo_CarreraPorAnio_DiceAnioEnVezDeCuatrimestre()
    {
        var cuerpo = ConstanciaRegularFormatter.Cuerpo(Contexto(porAnio: true, cuatrimestre: 2, codigoCarrera: "333", turno: 1));

        var texto = string.Join("\n", cuerpo);
        Assert.Contains("- segundo año.", texto);
        Assert.DoesNotContain("cuatrimestre.", texto);
    }

    [Fact]
    public void Cuerpo_ADistancia_MencionaDictamenYNoLlevaHorario()
    {
        var cuerpo = ConstanciaRegularFormatter.Cuerpo(Contexto(aDistancia: true, cuatrimestre: 1, dictamen: "1234"));

        var texto = string.Join("\n", cuerpo);
        Assert.Contains("es alumno regular del primer cuatrimestre de la carrera", texto);
        Assert.Contains("Dictamen del Consejo Federal de Educación", texto);
        Assert.Contains("N° 1234.", texto);
        Assert.DoesNotContain("Asiste los dias", texto);
    }

    [Theory]
    [InlineData("BAC", 1, "8:30 a 11:30")]
    [InlineData("BAC", 4, "19:00 a 22:00")]
    [InlineData("333", 1, "8:00 a 13:00")]
    [InlineData("650", 2, "13:00 a 18:00")]
    [InlineData("XYZ", 3, "17:15 a 20:00")]
    public void Horario_SegunCarreraYTurno_DevuelveElTextoEsperado(string carrera, int turno, string esperado)
    {
        Assert.Contains(esperado, ConstanciaRegularFormatter.Horario(carrera, turno));
    }

    [Theory]
    [InlineData("333", 3)]
    [InlineData("BAC", 9)]
    public void Horario_TurnoFueraDeTabla_DevuelveSinHorarioDefinido(string carrera, int turno)
    {
        Assert.Equal("Sin horario definido", ConstanciaRegularFormatter.Horario(carrera, turno));
    }

    [Theory]
    [InlineData("TER", "80%")]
    [InlineData("BAC", "70%")]
    public void LineaSubvencion_TerOBac_DevuelveElPorcentaje(string tipo, string porcentaje)
    {
        Assert.Contains(porcentaje, ConstanciaRegularFormatter.LineaSubvencion(tipo));
    }

    [Fact]
    public void LineaSubvencion_OtroTipo_DevuelveNull()
    {
        Assert.Null(ConstanciaRegularFormatter.LineaSubvencion("UNI"));
    }
}
