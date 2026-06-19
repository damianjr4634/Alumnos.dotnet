using Esba.Domain.Certificados;

namespace Esba.Domain.Tests.Certificados;

public class ConstanciaMateriasAprobadasFormatterTests
{
    private static MateriaAnaliticoConstancia Materia(
        string? condicion = null,
        bool esAnual = false,
        decimal? nota = null,
        DateOnly? fecha = null,
        string? instituto = null,
        string? caracteristica = null,
        string? actInt = null,
        string? actDegp = null,
        string? eximDesc = null) => new()
        {
            Cuatrimestre = 1,
            Descripcion = "Materia X",
            EsAnual = esAnual,
            Condicion = condicion,
            Nota = nota,
            Fecha = fecha,
            Instituto = instituto,
            Caracteristica = caracteristica,
            ActividadInterna = actInt,
            ActividadDgegp = actDegp,
            EximidoDescripcion = eximDesc,
        };

    [Fact]
    public void FormatearFila_MateriaAprobada_LlevaCondicionNotaFechaInstituto()
    {
        var fila = ConstanciaMateriasAprobadasFormatter.FormatearFila(
            Materia(condicion: "APROBADA", nota: 8.50m, fecha: new DateOnly(2024, 3, 15),
                instituto: "ESBA", caracteristica: "A-781"));

        Assert.False(fila.OcupaFilaCompleta);
        Assert.Equal("APROBADA", fila.Condicion);
        Assert.Equal("8.50", fila.Nota);
        Assert.Equal("15/03/2024", fila.Fecha);
        Assert.Equal("ESBA A-781", fila.Instituto);
    }

    [Fact]
    public void FormatearFila_Adeuda_MuestraAdeudaYGuionesEnLasColumnas()
    {
        var fila = ConstanciaMateriasAprobadasFormatter.FormatearFila(Materia(condicion: "* ADEUDA *"));

        Assert.False(fila.OcupaFilaCompleta);
        Assert.Equal("ADEUDA", fila.Condicion);
        Assert.Equal("—", fila.Nota);
        Assert.Equal("—", fila.Fecha);
        Assert.Equal("—", fila.Instituto);
    }

    [Fact]
    public void FormatearFila_NotaCero_SeMuestraComoSinDato()
    {
        var fila = ConstanciaMateriasAprobadasFormatter.FormatearFila(Materia(condicion: "REGULAR", nota: 0m));

        Assert.Equal("—", fila.Nota);
    }

    [Fact]
    public void FormatearFila_Anual_OcupaLaFilaConTextoUnico()
    {
        var fila = ConstanciaMateriasAprobadasFormatter.FormatearFila(Materia(esAnual: true, condicion: "REGULAR"));

        Assert.True(fila.OcupaFilaCompleta);
        Assert.Equal("MATERIA ANUAL", fila.Condicion);
        Assert.Equal(string.Empty, fila.Nota);
    }

    [Fact]
    public void FormatearFila_Eximido_UsaLaDescripcionDeEximicion()
    {
        var fila = ConstanciaMateriasAprobadasFormatter.FormatearFila(
            Materia(condicion: "EXIMIDO", eximDesc: "Eximida por Res. 123/24"));

        Assert.True(fila.OcupaFilaCompleta);
        Assert.Equal("Eximida por Res. 123/24", fila.Condicion);
    }

    [Fact]
    public void FormatearFila_EquivalenciaConActaInterna_UsaActInt()
    {
        var fila = ConstanciaMateriasAprobadasFormatter.FormatearFila(
            Materia(condicion: "EQUIVALENCIA", actInt: "457"));

        Assert.True(fila.OcupaFilaCompleta);
        Assert.Equal("APROBADA POR EQUIVALENCIA - Act. Interna N° 457", fila.Condicion);
    }

    [Fact]
    public void FormatearFila_EquivalenciaSinActaInterna_CaeEnActDegp()
    {
        var fila = ConstanciaMateriasAprobadasFormatter.FormatearFila(
            Materia(condicion: "EQUIVALENCIA", actDegp: "99/24"));

        Assert.True(fila.OcupaFilaCompleta);
        Assert.Equal("APROBADA POR EQUIVALENCIA - Act. D.G.E.G.P. N° 99/24", fila.Condicion);
    }

    [Fact]
    public void Formatear_PreservaElOrdenDeEntrada()
    {
        var entrada = new List<MateriaAnaliticoConstancia>
        {
            Materia(condicion: "APROBADA") with { Cuatrimestre = 1, Descripcion = "A" },
            Materia(condicion: "APROBADA") with { Cuatrimestre = 2, Descripcion = "B" },
        };

        var filas = ConstanciaMateriasAprobadasFormatter.Formatear(entrada);

        Assert.Equal(["A", "B"], filas.Select(f => f.Materia));
        Assert.Equal([1, 2], filas.Select(f => f.Cuatrimestre));
    }
}
