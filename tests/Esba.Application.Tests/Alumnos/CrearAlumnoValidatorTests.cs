using Esba.Application.DTOs.Alumnos;
using Esba.Application.Validators;
using Esba.Domain.Enums;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Alumnos;

public class CrearAlumnoValidatorTests
{
    private readonly CrearAlumnoValidator _validator = new();

    private static CrearAlumnoCommand ComandoValido() => new()
    {
        TipoDocumento = "DNI",
        NumeroDocumento = "30123456",
        CodigoCarrera = "ADM",
        Apellido = "García",
        Nombre = "Ana",
        Estado = EstadoAlumno.Cursando,
    };

    [Fact]
    public void Validar_ComandoValido_Pasa()
    {
        _validator.TestValidate(ComandoValido()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_DocumentoNoNumerico_Falla()
    {
        var resultado = _validator.TestValidate(ComandoValido() with { NumeroDocumento = "12A456" });

        resultado.ShouldHaveValidationErrorFor(c => c.NumeroDocumento);
    }

    [Fact]
    public void Validar_TipoDocumentoInvalido_Falla()
    {
        var resultado = _validator.TestValidate(ComandoValido() with { TipoDocumento = "XX" });

        resultado.ShouldHaveValidationErrorFor(c => c.TipoDocumento);
    }

    [Fact]
    public void Validar_SinApellido_Falla()
    {
        var resultado = _validator.TestValidate(ComandoValido() with { Apellido = "" });

        resultado.ShouldHaveValidationErrorFor(c => c.Apellido);
    }

    [Fact]
    public void Validar_MailInvalido_Falla()
    {
        var resultado = _validator.TestValidate(ComandoValido() with { Mail = "no-es-mail" });

        resultado.ShouldHaveValidationErrorFor(c => c.Mail);
    }

    [Fact]
    public void Validar_FechaNacimientoFutura_Falla()
    {
        var resultado = _validator.TestValidate(ComandoValido() with
        {
            FechaNacimiento = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
        });

        resultado.ShouldHaveValidationErrorFor(c => c.FechaNacimiento);
    }

    [Fact]
    public void Validar_ApellidoMuyLargo_Falla()
    {
        var resultado = _validator.TestValidate(ComandoValido() with { Apellido = new string('a', 26) });

        resultado.ShouldHaveValidationErrorFor(c => c.Apellido);
    }
}
