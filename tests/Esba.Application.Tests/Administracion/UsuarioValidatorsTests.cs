using Esba.Application.DTOs.Administracion;
using Esba.Application.Validators;
using Esba.Domain.Enums;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.Administracion;

public class UsuarioValidatorsTests
{
    private readonly CrearUsuarioValidator _crear = new();
    private readonly ActualizarUsuarioValidator _actualizar = new();

    private static CrearUsuarioCommand AltaValida() => new()
    {
        NombreUsuario = "jperez",
        Password = "clave123",
        Nombres = "Juan",
        Apellido = "Pérez",
        Cargo = "Bedel",
    };

    private static ActualizarUsuarioCommand ModifValida() => new()
    {
        Codigo = 7,
        NombreUsuario = "jperez",
        Nombres = "Juan",
        Apellido = "Pérez",
        Cargo = "Bedel",
    };

    [Fact]
    public void Crear_ComandoValido_Pasa() =>
        _crear.TestValidate(AltaValida()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Crear_SinNombre_Falla() =>
        _crear.TestValidate(AltaValida() with { NombreUsuario = "" })
            .ShouldHaveValidationErrorFor(c => c.NombreUsuario);

    [Fact]
    public void Crear_NombreDemasiadoLargo_Falla() =>
        _crear.TestValidate(AltaValida() with { NombreUsuario = new string('x', 16) })
            .ShouldHaveValidationErrorFor(c => c.NombreUsuario);

    [Fact]
    public void Crear_SinPassword_Falla() =>
        _crear.TestValidate(AltaValida() with { Password = "" })
            .ShouldHaveValidationErrorFor(c => c.Password);

    [Fact]
    public void Crear_PasswordCorta_Falla() =>
        _crear.TestValidate(AltaValida() with { Password = "abc" })
            .ShouldHaveValidationErrorFor(c => c.Password);

    [Fact]
    public void Crear_ApellidoDemasiadoLargo_Falla() =>
        _crear.TestValidate(AltaValida() with { Apellido = new string('x', 51) })
            .ShouldHaveValidationErrorFor(c => c.Apellido);

    [Fact]
    public void Crear_DocenteConCodigo_Pasa() =>
        _crear.TestValidate(AltaValida() with { Tipo = TipoUsuario.Docente, CodigoDocente = "017" })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Crear_DocenteSinCodigo_Falla() =>
        _crear.TestValidate(AltaValida() with { Tipo = TipoUsuario.Docente })
            .ShouldHaveValidationErrorFor("CodigoDocente");

    [Fact]
    public void Crear_DocenteConCodigoLargo_Falla() =>
        _crear.TestValidate(AltaValida() with { Tipo = TipoUsuario.Docente, CodigoDocente = "0017" })
            .ShouldHaveValidationErrorFor("CodigoDocente");

    [Fact]
    public void Crear_SecretariaSinCodigoDocente_NoExigeVinculo() =>
        _crear.TestValidate(AltaValida() with { Tipo = TipoUsuario.Secretaria })
            .ShouldNotHaveValidationErrorFor("CodigoDocente");

    [Fact]
    public void Crear_DocenteSupervisor_Falla() =>
        _crear.TestValidate(AltaValida() with { Tipo = TipoUsuario.Docente, CodigoDocente = "017", EsSupervisor = true })
            .ShouldHaveValidationErrorFor("EsSupervisor");

    [Fact]
    public void Crear_SecretariaSupervisor_Pasa() =>
        _crear.TestValidate(AltaValida() with { EsSupervisor = true })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Crear_AlumnoConCarreraYCodigo_Pasa() =>
        _crear.TestValidate(AltaValida() with { Tipo = TipoUsuario.Alumno, AlumnoCarrera = "TER", AlumnoCodigo = "12345678" })
            .ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Crear_AlumnoSinCarrera_Falla() =>
        _crear.TestValidate(AltaValida() with { Tipo = TipoUsuario.Alumno, AlumnoCodigo = "12345678" })
            .ShouldHaveValidationErrorFor("AlumnoCarrera");

    [Fact]
    public void Crear_AlumnoSinCodigo_Falla() =>
        _crear.TestValidate(AltaValida() with { Tipo = TipoUsuario.Alumno, AlumnoCarrera = "TER" })
            .ShouldHaveValidationErrorFor("AlumnoCodigo");

    [Fact]
    public void Crear_TipoFueraDelEnum_Falla() =>
        _crear.TestValidate(AltaValida() with { Tipo = (TipoUsuario)99 })
            .ShouldHaveValidationErrorFor("Tipo");

    [Fact]
    public void Actualizar_ComandoValido_Pasa() =>
        _actualizar.TestValidate(ModifValida()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Actualizar_CodigoInvalido_Falla() =>
        _actualizar.TestValidate(ModifValida() with { Codigo = 0 })
            .ShouldHaveValidationErrorFor(c => c.Codigo);

    [Fact]
    public void Actualizar_SinNombre_Falla() =>
        _actualizar.TestValidate(ModifValida() with { NombreUsuario = "" })
            .ShouldHaveValidationErrorFor(c => c.NombreUsuario);

    [Fact]
    public void Actualizar_CargoDemasiadoLargo_Falla() =>
        _actualizar.TestValidate(ModifValida() with { Cargo = new string('x', 31) })
            .ShouldHaveValidationErrorFor(c => c.Cargo);

    [Fact]
    public void Actualizar_DocenteSinCodigo_Falla() =>
        _actualizar.TestValidate(ModifValida() with { Tipo = TipoUsuario.Docente })
            .ShouldHaveValidationErrorFor("CodigoDocente");

    [Fact]
    public void Actualizar_DocenteSupervisor_Falla() =>
        _actualizar.TestValidate(ModifValida() with { Tipo = TipoUsuario.Docente, CodigoDocente = "017", EsSupervisor = true })
            .ShouldHaveValidationErrorFor("EsSupervisor");
}
