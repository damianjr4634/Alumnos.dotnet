using Esba.Application.Features.Administracion;
using Esba.Application.Validators;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Administracion;

public class IniciarSesionValidatorTests
{
    private readonly IniciarSesionValidator _validator = new();

    [Fact]
    public void Validar_ComandoValido_Pasa()
    {
        var resultado = _validator.TestValidate(new IniciarSesionCommand { NombreUsuario = "damian", Password = "clave" });

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_NombreUsuarioVacio_Falla()
    {
        var resultado = _validator.TestValidate(new IniciarSesionCommand { NombreUsuario = "", Password = "clave" });

        resultado.ShouldHaveValidationErrorFor(c => c.NombreUsuario);
    }

    [Fact]
    public void Validar_NombreUsuarioMuyLargo_Falla()
    {
        var resultado = _validator.TestValidate(new IniciarSesionCommand { NombreUsuario = new string('a', 16), Password = "clave" });

        resultado.ShouldHaveValidationErrorFor(c => c.NombreUsuario);
    }

    [Fact]
    public void Validar_PasswordVacia_Falla()
    {
        var resultado = _validator.TestValidate(new IniciarSesionCommand { NombreUsuario = "damian", Password = "" });

        resultado.ShouldHaveValidationErrorFor(c => c.Password);
    }
}
