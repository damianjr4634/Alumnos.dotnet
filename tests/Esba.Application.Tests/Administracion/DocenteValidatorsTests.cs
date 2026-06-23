using Esba.Application.DTOs.Academica;
using Esba.Application.Validators;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Administracion;

public class DocenteValidatorsTests
{
    private readonly CrearDocenteValidator _crear = new();
    private readonly ActualizarDocenteValidator _actualizar = new();

    private static CrearDocenteCommand AltaValida() => new()
    {
        Codigo = "012",
        Nombre = "Pérez, Juan",
    };

    private static ActualizarDocenteCommand ModifValida() => new()
    {
        Codigo = "012",
        Nombre = "Pérez, Juan",
    };

    [Fact]
    public void Crear_ComandoValido_Pasa() =>
        _crear.TestValidate(AltaValida()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Crear_SinCodigo_Falla() =>
        _crear.TestValidate(AltaValida() with { Codigo = "" })
            .ShouldHaveValidationErrorFor(c => c.Codigo);

    [Fact]
    public void Crear_CodigoDemasiadoLargo_Falla() =>
        _crear.TestValidate(AltaValida() with { Codigo = "1234" })
            .ShouldHaveValidationErrorFor(c => c.Codigo);

    [Fact]
    public void Crear_SinNombre_Falla() =>
        _crear.TestValidate(AltaValida() with { Nombre = "" })
            .ShouldHaveValidationErrorFor(c => c.Nombre);

    [Fact]
    public void Crear_DocumentoDemasiadoLargo_Falla() =>
        _crear.TestValidate(AltaValida() with { NumeroDocumento = "123456789" })
            .ShouldHaveValidationErrorFor(c => c.NumeroDocumento);

    [Fact]
    public void Actualizar_ComandoValido_Pasa() =>
        _actualizar.TestValidate(ModifValida()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Actualizar_SinNombre_Falla() =>
        _actualizar.TestValidate(ModifValida() with { Nombre = "" })
            .ShouldHaveValidationErrorFor(c => c.Nombre);
}
