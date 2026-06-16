using Esba.Domain.Certificados;

namespace Esba.Domain.Tests.Certificados;

public class MateriasAdeudadasCalculatorTests
{
    private static MateriaConstancia Materia(int cuatrimestre, string sigla, string? condicion, decimal? nota = null) =>
        new() { Cuatrimestre = cuatrimestre, Sigla = sigla, Condicion = condicion, Nota = nota };

    [Fact]
    public void Calcular_SinMateriasPendientes_DevuelveNinguna()
    {
        var resultado = MateriasAdeudadasCalculator.Calcular([], cuatrimestreMaximo: 0, esCarreraPorAnio: false);

        Assert.Equal("NINGUNA", resultado);
    }

    [Fact]
    public void Calcular_UnaMateriaPendiente_LaListaConCuatrimestre()
    {
        var materias = new[] { Materia(1, "MAT", "* ADEUDA *") };

        var resultado = MateriasAdeudadasCalculator.Calcular(materias, cuatrimestreMaximo: 0, esCarreraPorAnio: false);

        Assert.Equal("MAT DEL PRIMER CUAT.", resultado);
    }

    [Fact]
    public void Calcular_CarreraPorAnio_UsaSeparadorAnio()
    {
        var materias = new[] { Materia(1, "MAT", "CURSANDO") };

        var resultado = MateriasAdeudadasCalculator.Calcular(materias, cuatrimestreMaximo: 0, esCarreraPorAnio: true);

        // El separador legacy ' AÑO; ' (a diferencia de ' CUAT.; ') no lleva punto.
        Assert.Equal("MAT DEL PRIMER AÑO", resultado);
    }

    [Fact]
    public void Calcular_CincoPendientesEnPrimerCuatrimestre_ColapsaATodasLas()
    {
        var materias = Enumerable.Range(1, 5)
            .Select(i => Materia(1, $"M{i}", "* ADEUDA *"))
            .ToList();

        var resultado = MateriasAdeudadasCalculator.Calcular(materias, cuatrimestreMaximo: 0, esCarreraPorAnio: false);

        Assert.StartsWith("TODAS LAS", resultado);
        Assert.Contains("PRIMER CUAT.", resultado);
        Assert.DoesNotContain("M1", resultado);
    }

    [Fact]
    public void Calcular_VariosCuatrimestres_LosAgrupa()
    {
        var materias = new[]
        {
            Materia(1, "A", "* ADEUDA *"),
            Materia(2, "B", "* ADEUDA *"),
        };

        var resultado = MateriasAdeudadasCalculator.Calcular(materias, cuatrimestreMaximo: 0, esCarreraPorAnio: false);

        Assert.Contains("A DEL PRIMER CUAT.", resultado);
        Assert.Contains("B DEL SEGUNDO CUAT.", resultado);
    }

    [Fact]
    public void Calcular_ConTope_IgnoraCuatrimestresSuperiores()
    {
        var materias = new[]
        {
            Materia(1, "A", "* ADEUDA *"),
            Materia(2, "B", "* ADEUDA *"),
        };

        var resultado = MateriasAdeudadasCalculator.Calcular(materias, cuatrimestreMaximo: 1, esCarreraPorAnio: false);

        Assert.Contains("PRIMER", resultado);
        Assert.DoesNotContain("SEGUNDO", resultado);
    }

    [Theory]
    [InlineData("EQUIVALENCIA")]
    [InlineData("EXIMIDO")]
    public void Calcular_CondicionesAprobadas_NoCuentanComoAdeudadas(string condicion)
    {
        var materias = new[] { Materia(1, "MAT", condicion, nota: 8m) };

        var resultado = MateriasAdeudadasCalculator.Calcular(materias, cuatrimestreMaximo: 0, esCarreraPorAnio: false);

        Assert.Equal("NINGUNA", resultado);
    }

    [Fact]
    public void Calcular_NotaCeroAunqueRegular_CuentaComoAdeudada()
    {
        var materias = new[] { Materia(1, "MAT", "REGULAR", nota: 0m) };

        var resultado = MateriasAdeudadasCalculator.Calcular(materias, cuatrimestreMaximo: 0, esCarreraPorAnio: false);

        Assert.Contains("MAT", resultado);
    }

    [Fact]
    public void Calcular_MateriaConNotaAprobada_NoEsAdeudada()
    {
        var materias = new[] { Materia(1, "MAT", "REGULAR", nota: 7m) };

        var resultado = MateriasAdeudadasCalculator.Calcular(materias, cuatrimestreMaximo: 0, esCarreraPorAnio: false);

        Assert.Equal("NINGUNA", resultado);
    }
}
