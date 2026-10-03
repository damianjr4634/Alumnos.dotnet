using Esba.Application.DTOs.AreaDocente;
using Esba.Application.Validators;
using FluentValidation.TestHelper;

namespace Esba.Application.Tests.AreaDocente;

public class GuardarCargaComisionValidatorTests
{
    private readonly GuardarCargaComisionValidator _validator = new();

    private static GuardarCargaComisionCommand Valido(FilaCargaComisionInput? fila = null) => new()
    {
        Comision = new ClaveComision { CodigoCarrera = "TER", Cutuco = 101, CodigoMateria = "01", CuatrimestreAnio = "226" },
        Actor = new ActorCargaDocente { CodigoUsuario = 7, CodigoDocente = "017" },
        Filas = [fila ?? Fila()],
    };

    private static FilaCargaComisionInput Fila() => new()
    {
        CodigoAlumno = "30111222",
        Evaluacion1 = 7.5m,
        Recuperatorio = null,
        Evaluacion2 = 99m,
        TotalHoras = 64,
        Inasistencias = 6,
        Justificadas = 2,
    };

    [Fact]
    public void ComandoValido_Pasa() =>
        _validator.TestValidate(Valido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void SinFilas_Pasa_GuardarSoloObservaciones() =>
        _validator.TestValidate(Valido() with { Filas = [] }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void CarreraVacia_Falla() =>
        _validator.TestValidate(Valido() with { Comision = new ClaveComision { CodigoCarrera = "", Cutuco = 101, CodigoMateria = "01", CuatrimestreAnio = "226" } })
            .ShouldHaveValidationErrorFor("Comision.CodigoCarrera");

    [Fact]
    public void CutucoCero_Falla() =>
        _validator.TestValidate(Valido() with { Comision = new ClaveComision { CodigoCarrera = "TER", Cutuco = 0, CodigoMateria = "01", CuatrimestreAnio = "226" } })
            .ShouldHaveValidationErrorFor("Comision.Cutuco");

    [Fact]
    public void UsuarioInvalido_Falla() =>
        _validator.TestValidate(Valido() with { Actor = new ActorCargaDocente { CodigoUsuario = 0 } })
            .ShouldHaveValidationErrorFor("Actor.CodigoUsuario");

    [Theory]
    [InlineData(0.5)]
    [InlineData(10.5)]
    [InlineData(50)]
    [InlineData(-1)]
    public void NotaFueraDeRango_Falla(double nota) =>
        _validator.TestValidate(Valido(Fila() with { Evaluacion1 = (decimal)nota }))
            .ShouldHaveValidationErrorFor("Filas[0].Evaluacion1");

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(99)]
    public void NotaEnRangoOAusente_Pasa(double nota) =>
        _validator.TestValidate(Valido(Fila() with { NotaRegular = (decimal)nota }))
            .ShouldNotHaveValidationErrorFor("Filas[0].NotaRegular");

    [Fact]
    public void InasistenciasNegativas_Falla() =>
        _validator.TestValidate(Valido(Fila() with { Inasistencias = -1 }))
            .ShouldHaveValidationErrorFor("Filas[0].Inasistencias");

    [Fact]
    public void FilaSinAlumno_Falla() =>
        _validator.TestValidate(Valido(Fila() with { CodigoAlumno = "" }))
            .ShouldHaveValidationErrorFor("Filas[0].CodigoAlumno");

    [Fact]
    public void ObservacionDemasiadoLarga_Falla() =>
        _validator.TestValidate(Valido() with { Observaciones = new string('x', 501) })
            .ShouldHaveValidationErrorFor(c => c.Observaciones);
}
