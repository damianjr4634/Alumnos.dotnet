using Esba.Application.DTOs.Examenes;
using FluentValidation;

namespace Esba.Application.Validators;

internal static class MesaReglas
{
    public static void AplicarReglasComunes<T>(this AbstractValidator<T> validator)
        where T : IMesaCampos
    {
        validator.RuleFor(m => m.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        validator.RuleFor(m => m.NumeroMesa).GreaterThan(0).WithMessage("El número de mesa debe ser mayor a 0.");
        validator.RuleFor(m => m.CodigoMateria).NotEmpty().WithMessage("La materia es obligatoria.");
        validator.RuleFor(m => m.CodigoTipo).NotEmpty().WithMessage("El tipo de mesa es obligatorio.");
        validator.RuleFor(m => m.Llamado).GreaterThanOrEqualTo(0).WithMessage("El llamado no puede ser negativo.");
    }
}

public sealed class CrearMesaValidator : AbstractValidator<CrearMesaCommand>
{
    public CrearMesaValidator() => this.AplicarReglasComunes();
}

public sealed class ActualizarMesaValidator : AbstractValidator<ActualizarMesaCommand>
{
    public ActualizarMesaValidator() => this.AplicarReglasComunes();
}
