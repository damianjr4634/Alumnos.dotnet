using Esba.Application.DTOs.Academica;
using Esba.Application.Validators;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Academica;

public class ComisionValidatorTests
{
    private readonly CrearComisionValidator _validator = new();

    private static CrearComisionCommand ComandoValido() => new()
    {
        CodigoCarrera = "ADM",
        Cutuco = 111,
        CodigoMateria = "01",
        CuatrimestreAnio = "124",
    };

    [Fact]
    public void Validar_ComandoValido_Pasa()
    {
        _validator.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SinCarrera_Falla()
    {
        _validator.TestValidate(ComandoValido() with { CodigoCarrera = "" })
            .ShouldHaveValidationErrorFor(c => c.CodigoCarrera);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)1000)]
    public void Validar_CutucoFueraDeRango_Falla(short cutuco)
    {
        _validator.TestValidate(ComandoValido() with { Cutuco = cutuco })
            .ShouldHaveValidationErrorFor(c => c.Cutuco);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("1245")]
    public void Validar_CuatrimestreAnioInvalido_Falla(string cuaAnio)
    {
        _validator.TestValidate(ComandoValido() with { CuatrimestreAnio = cuaAnio })
            .ShouldHaveValidationErrorFor(c => c.CuatrimestreAnio);
    }

    [Fact]
    public void Validar_MasDeTresDiasConDictado_Falla()
    {
        var horario = new[]
        {
            new HorarioDiaComision { Dia = "LUNES", Primero = true },
            new HorarioDiaComision { Dia = "MARTES", Primero = true },
            new HorarioDiaComision { Dia = "MIERCOLES", Primero = true },
            new HorarioDiaComision { Dia = "JUEVES", Primero = true },
        };

        var resultado = _validator.TestValidate(ComandoValido() with { Horario = horario });

        Assert.False(resultado.IsValid);
    }

    [Fact]
    public void Validar_TresDiasConDictado_Pasa()
    {
        var horario = new[]
        {
            new HorarioDiaComision { Dia = "LUNES", Primero = true },
            new HorarioDiaComision { Dia = "MARTES", Segundo = true },
            new HorarioDiaComision { Dia = "MIERCOLES", Tercero = true },
        };

        _validator.TestValidate(ComandoValido() with { Horario = horario }).ShouldNotHaveAnyValidationErrors();
    }
}
