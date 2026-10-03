using Esba.Domain.Enums;

namespace Esba.Domain.Tests.Common;

/// <summary>Correspondencia ESTADO ('BOR'/'FIN'/'EFE') ↔ EstadoCargaDocente usada por EF y las queries.</summary>
public class EstadoCargaDocenteCodigoTests
{
    [Theory]
    [InlineData(EstadoCargaDocente.Borrador, "BOR")]
    [InlineData(EstadoCargaDocente.Finalizada, "FIN")]
    [InlineData(EstadoCargaDocente.Efectivizada, "EFE")]
    public void IdaYVuelta_EsEstable(EstadoCargaDocente estado, string codigo)
    {
        Assert.Equal(codigo, EstadoCargaDocenteCodigo.ACodigo(estado));
        Assert.Equal(estado, EstadoCargaDocenteCodigo.DesdeCodigo(codigo));
        Assert.Equal(estado, EstadoCargaDocenteCodigo.DesdeCodigo(codigo.ToLowerInvariant() + " "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void DesdeCodigoOpcional_SinValor_EsNull(string? codigo) =>
        Assert.Null(EstadoCargaDocenteCodigo.DesdeCodigoOpcional(codigo));

    [Fact]
    public void DesdeCodigo_Desconocido_LanzaExcepcion() =>
        Assert.Throws<InvalidOperationException>(() => EstadoCargaDocenteCodigo.DesdeCodigo("XXX"));

    [Fact]
    public void Etiqueta_SinEstado_EsSinCargar() =>
        Assert.Equal("Sin cargar", EstadoCargaDocenteCodigo.Etiqueta(null));
}
