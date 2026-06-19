using Esba.Application.DTOs.Academica;
using Esba.Application.Validators;
using Esba.Domain.Enums;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Academica;

public class CrearEquivalenciaValidatorTests
{
    private readonly CrearEquivalenciaValidator _validator = new();

    private static CrearEquivalenciaCommand ComandoValido() => new()
    {
        CodigoCarrera = "TER",
        CodigoAlumno = "DNI30123456",
        CodigoMateria = "07",
        TipoActuacion = TipoActuacionEquivalencia.Interna,
        InstitutoOrigen = "Instituto Origen",
    };

    [Fact]
    public void Validar_ComandoValido_Pasa()
    {
        _validator.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_SinMateria_Falla()
    {
        _validator.TestValidate(ComandoValido() with { CodigoMateria = "" })
            .ShouldHaveValidationErrorFor(c => c.CodigoMateria);
    }

    [Fact]
    public void Validar_SinInstitutoOrigen_Falla()
    {
        _validator.TestValidate(ComandoValido() with { InstitutoOrigen = "  " })
            .ShouldHaveValidationErrorFor(c => c.InstitutoOrigen);
    }

    [Fact]
    public void Validar_Dgegp_SinNumero_Falla()
    {
        var comando = ComandoValido() with { TipoActuacion = TipoActuacionEquivalencia.Dgegp, NumeroDgegp = null };
        _validator.TestValidate(comando).ShouldHaveValidationErrorFor(c => c.NumeroDgegp);
    }

    [Fact]
    public void Validar_Interna_SinNumeroDgegp_NoExigeNumero()
    {
        // En interna el número lo asigna el sistema: no se exige NumeroDgegp.
        var comando = ComandoValido() with { TipoActuacion = TipoActuacionEquivalencia.Interna, NumeroDgegp = null };
        _validator.TestValidate(comando).ShouldNotHaveValidationErrorFor(c => c.NumeroDgegp);
    }
}
