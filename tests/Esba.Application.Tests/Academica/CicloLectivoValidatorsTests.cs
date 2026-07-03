using Esba.Application.DTOs.Academica;
using Esba.Application.Validators;

namespace Esba.Application.Tests.Academica;

public class CicloLectivoValidatorsTests
{
    private readonly GuardarCicloCuatrimestralValidator _cuatrimestral = new();
    private readonly GuardarCicloTrimestralValidator _trimestral = new();

    private static GuardarCicloCuatrimestralCommand Cuatrimestral2026() => new()
    {
        EsNuevo = true,
        Anio = 2026,
        PrimerCuatrimestreDesde = new DateOnly(2026, 3, 2),
        PrimerCuatrimestreHasta = new DateOnly(2026, 7, 10),
        SegundoCuatrimestreDesde = new DateOnly(2026, 8, 3),
        SegundoCuatrimestreHasta = new DateOnly(2026, 12, 4),
    };

    private static GuardarCicloTrimestralCommand Trimestral2026() => new()
    {
        EsNuevo = true,
        Anio = 2026,
        PrimerTrimestreDesde = new DateOnly(2026, 3, 2),
        PrimerTrimestreHasta = new DateOnly(2026, 5, 29),
        SegundoTrimestreDesde = new DateOnly(2026, 6, 1),
        SegundoTrimestreHasta = new DateOnly(2026, 9, 4),
        TercerTrimestreDesde = new DateOnly(2026, 9, 7),
        TercerTrimestreHasta = new DateOnly(2026, 12, 4),
    };

    [Fact]
    public void Cuatrimestral_Completo_EsValido() =>
        Assert.True(_cuatrimestral.Validate(Cuatrimestral2026()).IsValid);

    [Fact]
    public void Cuatrimestral_SinFecha_EsInvalido() =>
        Assert.False(_cuatrimestral.Validate(Cuatrimestral2026() with { SegundoCuatrimestreHasta = null }).IsValid);

    [Theory]
    [InlineData(1979)]
    [InlineData(2101)]
    public void Cuatrimestral_AnioFueraDeRango_EsInvalido(int anio) =>
        Assert.False(_cuatrimestral.Validate(Cuatrimestral2026() with { Anio = anio }).IsValid);

    [Fact]
    public void Cuatrimestral_PeriodoInvertido_EsInvalido()
    {
        var resultado = _cuatrimestral.Validate(Cuatrimestral2026() with
        {
            PrimerCuatrimestreHasta = new DateOnly(2026, 2, 27),
        });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage.Contains("termina antes de empezar"));
    }

    [Fact]
    public void Cuatrimestral_SegundoPisaAlPrimero_EsInvalido()
    {
        var resultado = _cuatrimestral.Validate(Cuatrimestral2026() with
        {
            SegundoCuatrimestreDesde = new DateOnly(2026, 7, 10),
        });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage.Contains("se superpone"));
    }

    [Fact]
    public void Trimestral_Completo_EsValido() =>
        Assert.True(_trimestral.Validate(Trimestral2026()).IsValid);

    [Fact]
    public void Trimestral_SinTercerTrimestre_EsInvalido() =>
        Assert.False(_trimestral.Validate(Trimestral2026() with { TercerTrimestreDesde = null }).IsValid);

    [Fact]
    public void Trimestral_TerceroPisaAlSegundo_EsInvalido()
    {
        var resultado = _trimestral.Validate(Trimestral2026() with
        {
            TercerTrimestreDesde = new DateOnly(2026, 9, 4),
        });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.ErrorMessage.Contains("3er trimestre se superpone"));
    }
}
