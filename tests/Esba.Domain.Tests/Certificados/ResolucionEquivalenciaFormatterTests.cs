using Esba.Domain.Certificados;

namespace Esba.Domain.Tests.Certificados;

public class ResolucionEquivalenciaFormatterTests
{
    [Theory]
    [InlineData("2,3", "2,3")]
    [InlineData("3, 2, 2", "2,3")]   // ordena y deduplica
    [InlineData("1;2 4", "1,2,4")]   // acepta separadores varios
    [InlineData("x, 2, ", "2")]      // descarta no numéricos
    [InlineData("", "")]
    [InlineData(null, "")]
    public void ParsearCuatrimestres_NormalizaLaEntrada(string? entrada, string esperado) =>
        Assert.Equal(esperado, string.Join(",", ResolucionEquivalenciaFormatter.ParsearCuatrimestres(entrada)));

    [Fact]
    public void TextoVisto_IncluyeAlumnoAnioCuatrimestresYCarrera()
    {
        int[] cuatrimestres = [2, 3];
        var texto = ResolucionEquivalenciaFormatter.TextoVisto(
            "Pérez Juan", "DNI 12345678", 2026, cuatrimestres, "Tecnicatura en RRHH");

        Assert.Contains("Pérez Juan DNI 12345678", texto);
        Assert.Contains("matriculado/a en año 2026", texto);
        Assert.Contains("cuatrimestre/s 2,3", texto);
        Assert.Contains("de la carrera Tecnicatura en RRHH", texto);
    }

    [Fact]
    public void TextoConsiderando_CierraConElRector()
    {
        var texto = ResolucionEquivalenciaFormatter.TextoConsiderando("Dra. López");

        Assert.Contains("Dirección General de Educación de Gestión Privada", texto);
        Assert.EndsWith("El/La Rector/a del Instituto Dra. López", texto);
    }

    [Fact]
    public void ParrafoMateria_ArmaElTextoConOrdinalYDatosDeOrigen()
    {
        var texto = ResolucionEquivalenciaFormatter.ParrafoMateria(
            "Filosofía y Lógica", 1, "204/19",
            "Filosofía y Lógica", "Tec. Sup. en RRHH", "Instituto X", "Lema, Gabriela");

        Assert.StartsWith("Materia Filosofía y Lógica del PRIMER cuatrimestre con Acta Interna N° 204/19.", texto);
        // corrige el faltante de espacio del legacy ("Establecimiento"+inst)
        Assert.Contains("en el Establecimiento Instituto X.", texto);
        Assert.EndsWith("evaluada por el docente Lema, Gabriela", texto);
    }
}
