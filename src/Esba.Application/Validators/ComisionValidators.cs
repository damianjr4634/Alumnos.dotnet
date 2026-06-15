using Esba.Application.DTOs.Academica;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Reglas comunes de validación de comisión. La superposición de horarios y la
/// coincidencia del cuatrimestre con la materia las valida el SP XXX_VAL_COMISION
/// tras insertar; acá van las reglas de forma previas a tocar la base (§2.4).
/// </summary>
internal static class ComisionReglas
{
    public static void AplicarReglasComunes<T>(this AbstractValidator<T> validator)
        where T : IComisionCampos
    {
        validator.RuleFor(c => c.CodigoCarrera)
            .NotEmpty().WithMessage("La carrera es obligatoria.");

        validator.RuleFor(c => c.Cutuco)
            .InclusiveBetween((short)1, (short)999)
            .WithMessage("La comisión (CUTUCO) debe tener entre 1 y 3 dígitos.");

        validator.RuleFor(c => c.CodigoMateria)
            .NotEmpty().WithMessage("La materia es obligatoria.")
            .Must(m => m is not null && m.Trim().Length <= 2)
            .WithMessage("El código de materia debe tener hasta 2 caracteres.");

        validator.RuleFor(c => c.CuatrimestreAnio)
            .NotEmpty().WithMessage("El cuatrimestre/año es obligatorio.")
            .Must(c => c is not null && c.Trim().Length == 3)
            .WithMessage("El cuatrimestre/año debe tener 3 dígitos (ej. 124).");

        validator.RuleFor(c => c.Horario)
            .Must(h => h.Count(d => d.TieneDictado) <= 3)
            .WithMessage("Una comisión no puede dictarse en más de 3 días.");
    }
}

public sealed class CrearComisionValidator : AbstractValidator<CrearComisionCommand>
{
    public CrearComisionValidator() => this.AplicarReglasComunes();
}

public sealed class ActualizarComisionValidator : AbstractValidator<ActualizarComisionCommand>
{
    public ActualizarComisionValidator() => this.AplicarReglasComunes();
}
