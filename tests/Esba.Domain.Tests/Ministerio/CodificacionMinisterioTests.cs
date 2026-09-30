using Esba.Domain.Ministerio;

namespace Esba.Domain.Tests.Ministerio;

/// <summary>
/// Codificación de las columnas del padrón al Ministerio (sucesor de las expresiones
/// IIF/CASE del SELECT de ComisionesAlMinisterio.pas).
/// </summary>
public class CodificacionMinisterioTests
{
    [Theory]
    [InlineData("TER", true)]
    [InlineData(" ter ", true)]
    [InlineData("BAC", false)]
    [InlineData(null, false)]
    public void EsTerciaria_SoloParaTipoTer(string? tipo, bool esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.EsTerciaria(tipo));
    }

    [Theory]
    [InlineData("TER", false, "P")]
    [InlineData("TER", true, "D")]
    [InlineData("BAC", false, "C")]
    [InlineData("BAC", true, "C")]
    [InlineData("BAD", false, "A")]
    [InlineData("333", false, "A")]
    [InlineData(null, false, "A")]
    public void Modalidad_TerciariaPorDistancia_SecundariaPorTipoBac(string? tipo, bool distancia, string esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.Modalidad(tipo, distancia));
    }

    [Theory]
    [InlineData("TECNICATURA SUPERIOR EN ADMINISTRACIÓN", "TC")]
    [InlineData("tecnicatura en gestión", "TC")]
    [InlineData("PROFESORADO DE INGLÉS", null)]
    [InlineData(null, null)]
    public void Orientacion_TcSoloParaTecnicaturas(string? descripcion, string? esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.Orientacion(descripcion));
    }

    [Theory]
    [InlineData(111, false, "TM")]
    [InlineData(121, false, "TT")]
    [InlineData(131, false, "TV")]
    [InlineData(141, false, "TN")]
    [InlineData(151, false, null)]
    [InlineData(111, true, "TD")]
    [InlineData(11, false, null)]
    public void Turno_PorSegundoDigito_ODistancia(int cutuco, bool distancia, string? esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.Turno(cutuco, distancia));
    }

    [Theory]
    [InlineData(111, "TEC", 1)]
    [InlineData(211, "TEC", 1)]
    [InlineData(311, "TEC", 2)]
    [InlineData(411, "TEC", 2)]
    [InlineData(511, "TEC", 3)]
    [InlineData(611, "TEC", 3)]
    [InlineData(711, "TEC", null)]
    [InlineData(311, "650", 3)]
    [InlineData(411, "650", 4)]
    [InlineData(11, "TEC", null)]
    public void AnioEstudio_PorParesDeCuatrimestres_Salvo650(int cutuco, string carrera, int? esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.AnioEstudio(cutuco, carrera));
    }

    [Theory]
    [InlineData(311, 3)]
    [InlineData(11, null)]
    public void Cuatrimestre_PrimerDigito(int cutuco, int? esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.Cuatrimestre(cutuco));
    }

    [Theory]
    [InlineData(111, "A")]
    [InlineData(112, "B")]
    [InlineData(114, "D")]
    [InlineData(116, "F")]
    [InlineData(117, null)]
    [InlineData(110, null)]
    [InlineData(11, null)]
    public void Division_TercerDigitoComoLetra(int cutuco, string? esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.Division(cutuco));
    }

    [Theory]
    [InlineData("RECURSANDO", "RC")]
    [InlineData(" recursando ", "RC")]
    [InlineData("CURSANDO", "R")]
    [InlineData(null, "R")]
    public void Condicion_RcSoloParaRecursantes(string? condicion, string esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.Condicion(condicion));
    }

    [Theory]
    [InlineData("F", "MUJER")]
    [InlineData("M", "VARON")]
    [InlineData(null, "VARON")]
    public void Genero_MujerSoloParaF(string? sexo, string esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.Genero(sexo));
    }

    [Fact]
    public void Documento_SeFormateaPorPosicionesFijasDeCodAlu()
    {
        Assert.Equal("DNI", CodificacionMinisterio.TipoDocumento("DNI12345678"));
        Assert.Equal("12.345.678", CodificacionMinisterio.NumeroDocumentoConPuntos("DNI12345678"));
        Assert.Equal("DNI 12.345.678", CodificacionMinisterio.DocumentoParaNomina("DNI12345678"));
    }

    [Fact]
    public void Documento_TipoDeDosLetras_ConservaLosCerosALaIzquierda()
    {
        Assert.Equal("CI", CodificacionMinisterio.TipoDocumento("CI 01234567"));
        Assert.Equal("01.234.567", CodificacionMinisterio.NumeroDocumentoConPuntos("CI 01234567"));
    }

    [Fact]
    public void Documento_CodigoCorto_NoFalla()
    {
        Assert.Equal("DN", CodificacionMinisterio.TipoDocumento("DN"));
        Assert.Equal("  .   .   ", CodificacionMinisterio.NumeroDocumentoConPuntos("DN"));
        Assert.Equal("  .   .   ", CodificacionMinisterio.NumeroDocumentoConPuntos(null));
    }

    [Theory]
    [InlineData("Colegio X", "Bachiller", "Colegio X Bachiller")]
    [InlineData("Colegio X", null, "Colegio X")]
    [InlineData(null, " Perito ", "Perito")]
    [InlineData(null, null, null)]
    [InlineData("  ", "", null)]
    public void TituloIngreso_ConcatenaColegioYTitulo(string? colegio, string? titulo, string? esperado)
    {
        Assert.Equal(esperado, CodificacionMinisterio.TituloIngreso(colegio, titulo));
    }

    [Fact]
    public void Edad_AniosCumplidos_RespetaElCumpleanios()
    {
        var hoy = new DateOnly(2026, 9, 29);

        Assert.Equal(20, CodificacionMinisterio.Edad(new DateOnly(2006, 9, 29), hoy));  // cumple hoy
        Assert.Equal(19, CodificacionMinisterio.Edad(new DateOnly(2006, 9, 30), hoy));  // cumple mañana
        Assert.Equal(26, CodificacionMinisterio.Edad(new DateOnly(2000, 2, 29), hoy));  // bisiesto
        Assert.Null(CodificacionMinisterio.Edad(null, hoy));
        Assert.Null(CodificacionMinisterio.Edad(new DateOnly(2027, 1, 1), hoy));
    }

    [Fact]
    public void NombreHoja_CarreraGuionCutuco()
    {
        Assert.Equal("TEC-111", CodificacionMinisterio.NombreHoja(" TEC ", 111));
    }
}
