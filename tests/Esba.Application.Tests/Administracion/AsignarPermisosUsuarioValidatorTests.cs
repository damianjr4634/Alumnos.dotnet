using Esba.Application.DTOs.Administracion;
using Esba.Application.Validators;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Administracion;

public class AsignarPermisosUsuarioValidatorTests
{
    private readonly AsignarPermisosUsuarioValidator _validator = new();

    private static AsignarPermisosUsuarioCommand Valido() => new()
    {
        CodigoUsuario = 7,
        CodigosOpcion = ["BAC"],
    };

    [Fact]
    public void Validar_ComandoValido_Pasa() =>
        _validator.TestValidate(Valido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validar_ListaVacia_Pasa() =>
        _validator.TestValidate(Valido() with { CodigosOpcion = [] }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validar_CodigoUsuarioInvalido_Falla() =>
        _validator.TestValidate(Valido() with { CodigoUsuario = 0 })
            .ShouldHaveValidationErrorFor(c => c.CodigoUsuario);

    [Fact]
    public void Validar_CodigosNull_Falla() =>
        _validator.TestValidate(Valido() with { CodigosOpcion = null! })
            .ShouldHaveValidationErrorFor(c => c.CodigosOpcion);
}
