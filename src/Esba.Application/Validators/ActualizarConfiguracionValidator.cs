using Esba.Application.DTOs.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class ActualizarConfiguracionValidator : AbstractValidator<ActualizarConfiguracionCommand>
{
    public ActualizarConfiguracionValidator()
    {
        RuleForEach(c => c.Valores).ChildRules(valor =>
        {
            valor.RuleFor(v => v.Parame)
                .NotEmpty().WithMessage("Hay un parámetro sin nombre.")
                .MaximumLength(30).WithMessage("El nombre del parámetro no puede superar los 30 caracteres.");

            valor.RuleFor(v => v.Valor)
                .MaximumLength(200).WithMessage("El valor no puede superar los 200 caracteres.");
        });
    }
}
