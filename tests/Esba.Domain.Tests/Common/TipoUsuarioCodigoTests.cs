using Esba.Domain.Enums;

namespace Esba.Domain.Tests.Common;

/// <summary>
/// Correspondencia USUARIOS.TIPO ↔ TipoUsuario: la usan el mapeo EF (lectura y
/// escritura) y los claims. Blanco = secretaría (DEFAULT de la columna, filas
/// previas a la migración 2026-10-02); un código desconocido falla fuerte.
/// </summary>
public class TipoUsuarioCodigoTests
{
    [Theory]
    [InlineData(TipoUsuario.Secretaria, "SEC")]
    [InlineData(TipoUsuario.Docente, "DOC")]
    [InlineData(TipoUsuario.Alumno, "ALU")]
    public void ACodigo_CadaTipo_DevuelveSuCodigoDeTresLetras(TipoUsuario tipo, string esperado) =>
        Assert.Equal(esperado, TipoUsuarioCodigo.ACodigo(tipo));

    [Theory]
    [InlineData("SEC", TipoUsuario.Secretaria)]
    [InlineData("DOC", TipoUsuario.Docente)]
    [InlineData("ALU", TipoUsuario.Alumno)]
    [InlineData("doc", TipoUsuario.Docente)]
    [InlineData("DOC ", TipoUsuario.Docente)]
    public void DesdeCodigo_CodigoConocido_DevuelveElTipo(string codigo, TipoUsuario esperado) =>
        Assert.Equal(esperado, TipoUsuarioCodigo.DesdeCodigo(codigo));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DesdeCodigo_NuloOBlanco_EsSecretaria(string? codigo) =>
        Assert.Equal(TipoUsuario.Secretaria, TipoUsuarioCodigo.DesdeCodigo(codigo));

    [Fact]
    public void DesdeCodigo_CodigoDesconocido_LanzaExcepcion() =>
        Assert.Throws<InvalidOperationException>(() => TipoUsuarioCodigo.DesdeCodigo("XYZ"));

    [Fact]
    public void ACodigo_ValorFueraDelEnum_LanzaExcepcion() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => TipoUsuarioCodigo.ACodigo((TipoUsuario)99));

    [Theory]
    [InlineData(TipoUsuario.Secretaria)]
    [InlineData(TipoUsuario.Docente)]
    [InlineData(TipoUsuario.Alumno)]
    public void IdaYVuelta_EsEstable(TipoUsuario tipo) =>
        Assert.Equal(tipo, TipoUsuarioCodigo.DesdeCodigo(TipoUsuarioCodigo.ACodigo(tipo)));
}
