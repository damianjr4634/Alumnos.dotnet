using Esba.Domain.Entities;

namespace Esba.Domain.Tests.Common;

/// <summary>
/// Separación de DOCENTES.DOCENTE ("APELLIDO, NOMBRES") para precargar el
/// usuario web del docente. Casos tomados de la convención real de la base.
/// </summary>
public class DocenteSepararNombreTests
{
    [Theory]
    [InlineData("BEJAR, EMILIO ROBERTO", "BEJAR", "EMILIO ROBERTO")]
    [InlineData("LANDONI DE BALDRICH, CONSTANZA", "LANDONI DE BALDRICH", "CONSTANZA")]
    [InlineData("PEREZ SAN MARTIN, MARIA NOEL", "PEREZ SAN MARTIN", "MARIA NOEL")]
    [InlineData("  ROSSI ,  JOSE IGNACIO  ", "ROSSI", "JOSE IGNACIO")]
    public void SepararApellidoYNombres_ConComa_CortaEnLaPrimeraComa(string docente, string apellido, string nombres)
    {
        var (a, n) = Docente.SepararApellidoYNombres(docente);

        Assert.Equal(apellido, a);
        Assert.Equal(nombres, n);
    }

    [Theory]
    [InlineData("DI BARTOLOMEO LAURA")]
    [InlineData("MONTAGNARO DIEGO EZEQUIEL")]
    public void SepararApellidoYNombres_SinComa_TodoVaAlApellido(string docente)
    {
        var (a, n) = Docente.SepararApellidoYNombres(docente);

        Assert.Equal(docente, a);
        Assert.Null(n);
    }

    [Fact]
    public void SepararApellidoYNombres_ComaSinNombres_DejaNombresNulo()
    {
        var (a, n) = Docente.SepararApellidoYNombres("GUASTELLA,");

        Assert.Equal("GUASTELLA", a);
        Assert.Null(n);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SepararApellidoYNombres_Vacio_DevuelveNulos(string? docente)
    {
        var (a, n) = Docente.SepararApellidoYNombres(docente);

        Assert.Null(a);
        Assert.Null(n);
    }
}
