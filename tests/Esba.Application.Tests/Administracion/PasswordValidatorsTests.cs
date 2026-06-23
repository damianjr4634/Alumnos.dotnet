using Esba.Application.DTOs.Administracion;
using Esba.Application.Validators;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Administracion;

public class PasswordValidatorsTests
{
    private readonly CambiarPasswordValidator _cambiar = new();
    private readonly BlanquearPasswordValidator _blanquear = new();

    private static CambiarPasswordCommand CambioValido() => new()
    {
        CodigoUsuario = 7,
        PasswordActual = "actual1",
        PasswordNueva = "nueva123",
        PasswordNuevaConfirmacion = "nueva123",
    };

    [Fact]
    public void Cambiar_ComandoValido_Pasa() =>
        _cambiar.TestValidate(CambioValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Cambiar_ActualVacia_Falla() =>
        _cambiar.TestValidate(CambioValido() with { PasswordActual = "" })
            .ShouldHaveValidationErrorFor(c => c.PasswordActual);

    [Fact]
    public void Cambiar_NuevaCorta_Falla() =>
        _cambiar.TestValidate(CambioValido() with { PasswordNueva = "abc", PasswordNuevaConfirmacion = "abc" })
            .ShouldHaveValidationErrorFor(c => c.PasswordNueva);

    [Fact]
    public void Cambiar_NuevaIgualALaActual_Falla() =>
        _cambiar.TestValidate(CambioValido() with { PasswordNueva = "actual1", PasswordNuevaConfirmacion = "actual1" })
            .ShouldHaveValidationErrorFor(c => c.PasswordNueva);

    [Fact]
    public void Cambiar_ConfirmacionNoCoincide_Falla() =>
        _cambiar.TestValidate(CambioValido() with { PasswordNuevaConfirmacion = "otra" })
            .ShouldHaveValidationErrorFor(c => c.PasswordNuevaConfirmacion);

    [Fact]
    public void Blanquear_ComandoValido_Pasa() =>
        _blanquear.TestValidate(new BlanquearPasswordCommand { CodigoUsuario = 7, PasswordTemporal = "temporal1" })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Blanquear_CodigoInvalido_Falla() =>
        _blanquear.TestValidate(new BlanquearPasswordCommand { CodigoUsuario = 0, PasswordTemporal = "temporal1" })
            .ShouldHaveValidationErrorFor(c => c.CodigoUsuario);

    [Fact]
    public void Blanquear_TemporalCorta_Falla() =>
        _blanquear.TestValidate(new BlanquearPasswordCommand { CodigoUsuario = 7, PasswordTemporal = "ab" })
            .ShouldHaveValidationErrorFor(c => c.PasswordTemporal);
}
