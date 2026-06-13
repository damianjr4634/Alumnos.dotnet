using Esba.Application.DTOs.Academica;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Reglas compartidas de inscripción. Condiciones válidas: las del combo de
/// alta del legacy (InscripcionDeMaterias.dfm), no las 14 históricas de los
/// datos. Turno y comisión son un dígito cada uno (forman el CUTUCO).
/// </summary>
internal static class InscripcionReglas
{
    public static readonly string[] CondicionesValidas = ["CURSANDO", "P/EQUIVALEN", "RECURSANDO"];
}

public sealed class InscribirEnMateriaValidator : AbstractValidator<InscribirEnMateriaCommand>
{
    public InscribirEnMateriaValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().MaximumLength(6);
        RuleFor(c => c.CodigoAlumno).NotEmpty().Length(11).WithMessage("Código de alumno inválido.");
        RuleFor(c => c.CodigoMateria).NotEmpty().MaximumLength(2).WithMessage("Seleccione la materia.");
        RuleFor(c => c.Turno).InclusiveBetween(0, 9).WithMessage("El turno debe ser un dígito (0 a 9).");
        RuleFor(c => c.NumeroComision).InclusiveBetween(0, 9).WithMessage("La comisión debe ser un dígito (0 a 9).");
        RuleFor(c => c.CuatrimestreAnio).Matches("^[0-9]{3}$")
            .WithMessage("El cuatrimestre/año debe tener formato CAA (ej. 124 = 1º/24).");
        RuleFor(c => c.Condicion)
            .Must(c => InscripcionReglas.CondicionesValidas.Contains(c?.Trim().ToUpperInvariant()))
            .WithMessage("Complete una condición (CURSANDO, P/EQUIVALEN o RECURSANDO).");
    }
}

public sealed class ModificarInscripcionValidator : AbstractValidator<ModificarInscripcionCommand>
{
    public ModificarInscripcionValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().MaximumLength(6);
        RuleFor(c => c.CodigoAlumno).NotEmpty().Length(11).WithMessage("Código de alumno inválido.");
        RuleFor(c => c.CodigoMateria).NotEmpty().MaximumLength(2);
        RuleFor(c => c.Turno).InclusiveBetween(0, 9).WithMessage("El turno debe ser un dígito (0 a 9).");
        RuleFor(c => c.NumeroComision).InclusiveBetween(0, 9).WithMessage("La comisión debe ser un dígito (0 a 9).");
        RuleFor(c => c.CuatrimestreAnio).Matches("^[0-9]{3}$")
            .WithMessage("El cuatrimestre/año debe tener formato CAA (ej. 124 = 1º/24).");
        RuleFor(c => c.Condicion)
            .Must(c => InscripcionReglas.CondicionesValidas.Contains(c?.Trim().ToUpperInvariant()))
            .WithMessage("Complete una condición (CURSANDO, P/EQUIVALEN o RECURSANDO).");
    }
}
