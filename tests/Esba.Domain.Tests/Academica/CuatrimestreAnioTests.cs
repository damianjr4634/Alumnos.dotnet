using Esba.Domain.Academica;

namespace Esba.Domain.Tests.Academica;

/// <summary>CUA_ANIO "cuatrimestre + año": etiqueta legible y orden cronológico, tolerante a códigos fuera del patrón.</summary>
public class CuatrimestreAnioTests
{
    [Theory]
    [InlineData("226", "2º cuatrimestre 2026")]
    [InlineData("124", "1º cuatrimestre 2024")]
    [InlineData(" 124 ", "1º cuatrimestre 2024")]
    public void Etiqueta_CodigoValido_DevuelveTextoLegible(string codigo, string esperado) =>
        Assert.Equal(esperado, CuatrimestreAnio.Etiqueta(codigo));

    [Theory]
    [InlineData("505")]   // cuatrimestre 5: fuera del patrón, se muestra tal cual
    [InlineData("300")]
    [InlineData("AB1")]
    [InlineData("12")]
    public void Etiqueta_CodigoFueraDelPatron_DevuelveElCodigo(string codigo) =>
        Assert.Equal(codigo, CuatrimestreAnio.Etiqueta(codigo));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Etiqueta_Vacio_DevuelveVacio(string? codigo) =>
        Assert.Equal(string.Empty, CuatrimestreAnio.Etiqueta(codigo));

    private static readonly string[] Desordenados = ["125", "224", "124", "505", "226"];
    private static readonly string[] EsperadosDescendente = ["226", "125", "224", "124", "505"];

    [Fact]
    public void ClaveOrden_OrdenaPorAnioYLuegoCuatrimestre()
    {
        // "224" (2/24) va antes que "125" (1/25) aunque lexicográficamente sea mayor.
        var ordenados = Desordenados.OrderByDescending(CuatrimestreAnio.ClaveOrden).ToArray();

        Assert.Equal(EsperadosDescendente, ordenados);
    }

    [Fact]
    public void ClaveOrden_CodigoFueraDelPatron_EsCero() =>
        Assert.Equal(0, CuatrimestreAnio.ClaveOrden("505"));
}
