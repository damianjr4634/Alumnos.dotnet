using Esba.Application.DTOs.Academica;
using Esba.Application.Validators;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Academica;

public class MateriaValidatorTests
{
    private readonly CrearMateriaValidator _crear = new();
    private readonly ActualizarMateriaValidator _actualizar = new();

    private static CrearMateriaCommand ComandoValido() => new()
    {
        CodigoCarrera = "ADM",
        Codigo = "01",
        Nombre = "Matemática I",
        Sigla = "MAT1",
        Cuatrimestre = 1,
        Orden = 1,
    };

    [Fact]
    public void Validar_ComandoValido_Pasa()
    {
        _crear.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SinCarrera_Falla()
    {
        _crear.TestValidate(ComandoValido() with { CodigoCarrera = "" })
            .ShouldHaveValidationErrorFor(m => m.CodigoCarrera);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Validar_CodigoInvalido_Falla(string codigo)
    {
        _crear.TestValidate(ComandoValido() with { Codigo = codigo })
            .ShouldHaveValidationErrorFor(m => m.Codigo);
    }

    [Fact]
    public void Validar_SinDescripcion_Falla()
    {
        _crear.TestValidate(ComandoValido() with { Nombre = "" })
            .ShouldHaveValidationErrorFor(m => m.Nombre);
    }

    [Fact]
    public void Validar_SinSigla_Falla()
    {
        _crear.TestValidate(ComandoValido() with { Sigla = "" })
            .ShouldHaveValidationErrorFor(m => m.Sigla);
    }

    [Fact]
    public void Validar_CuatrimestreCero_Falla()
    {
        _crear.TestValidate(ComandoValido() with { Cuatrimestre = 0 })
            .ShouldHaveValidationErrorFor(m => m.Cuatrimestre);
    }

    [Fact]
    public void Validar_OrdenNegativo_Falla()
    {
        _crear.TestValidate(ComandoValido() with { Orden = -1 })
            .ShouldHaveValidationErrorFor(m => m.Orden);
    }

    [Fact]
    public void Validar_PromocionYAprSinFinalALaVez_Falla()
    {
        var resultado = _crear.TestValidate(
            ComandoValido() with { AdmitePromocion = true, ApruebaSinFinal = true });

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Validar_SoloPromocion_Pasa()
    {
        _crear.TestValidate(ComandoValido() with { AdmitePromocion = true, ApruebaSinFinal = false })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidarActualizar_MismaReglaPromocionExcluyente_Falla()
    {
        var comando = new ActualizarMateriaCommand
        {
            CodigoCarrera = "ADM",
            Codigo = "01",
            Nombre = "Matemática I",
            Sigla = "MAT1",
            Cuatrimestre = 1,
            Orden = 1,
            AdmitePromocion = true,
            ApruebaSinFinal = true,
        };

        Assert.False(_actualizar.TestValidate(comando).IsValid);
    }
}
