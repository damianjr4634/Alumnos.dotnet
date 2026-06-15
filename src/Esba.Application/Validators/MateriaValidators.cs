using Esba.Application.DTOs.Academica;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Reglas comunes de validación de materia (alta y modificación). Centraliza la
/// regla de negocio del legacy (altamodifmaterias.BotonGrabarClick): una materia
/// no puede ser promocional y aprobarse sin final a la vez.
/// </summary>
internal static class MateriaReglas
{
    public static void AplicarReglasComunes<T>(this AbstractValidator<T> validator)
        where T : IMateriaCampos
    {
        validator.RuleFor(m => m.CodigoCarrera)
            .NotEmpty().WithMessage("La carrera es obligatoria.");

        validator.RuleFor(m => m.Codigo)
            .NotEmpty().WithMessage("El código de materia es obligatorio.")
            .Must(c => c is not null && c.Trim().Length is >= 1 and <= 2)
            .WithMessage("El código de materia debe tener 1 o 2 caracteres.");

        validator.RuleFor(m => m.Nombre)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(60).WithMessage("La descripción no puede superar los 60 caracteres.");

        validator.RuleFor(m => m.Sigla)
            .NotEmpty().WithMessage("La sigla es obligatoria.")
            .MaximumLength(30).WithMessage("La sigla no puede superar los 30 caracteres.");

        validator.RuleFor(m => m.Cuatrimestre)
            .GreaterThan((short)0).WithMessage("El cuatrimestre debe ser mayor a 0.");

        validator.RuleFor(m => m.Orden)
            .GreaterThanOrEqualTo((short)0).WithMessage("El orden no puede ser negativo.");

        validator.RuleFor(m => m.CodigoEquivalencia)
            .Must(c => string.IsNullOrWhiteSpace(c) || c.Trim().Length <= 2)
            .WithMessage("El código de equivalencia debe tener hasta 2 caracteres.");

        validator.RuleFor(m => m)
            .Must(m => !(m.AdmitePromocion && m.ApruebaSinFinal))
            .WithMessage("La materia no puede ser promocional y aprobarse sin final a la vez.");
    }
}

public sealed class CrearMateriaValidator : AbstractValidator<CrearMateriaCommand>
{
    public CrearMateriaValidator() => this.AplicarReglasComunes();
}

public sealed class ActualizarMateriaValidator : AbstractValidator<ActualizarMateriaCommand>
{
    public ActualizarMateriaValidator() => this.AplicarReglasComunes();
}
