using Esba.Application.DTOs.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class EnviarCorreoComisionValidator : AbstractValidator<EnviarCorreoComisionCommand>
{
    public EnviarCorreoComisionValidator()
    {
        RuleFor(c => c)
            .Must(c => c.Para.Count + c.CopiaCarbon.Count + c.CopiaOculta.Count > 0)
            .WithMessage("Seleccioná al menos un destinatario.");

        RuleFor(c => c.Asunto)
            .NotEmpty().WithMessage("Ingresá el asunto del mensaje.")
            .MaximumLength(200).WithMessage("El asunto no puede superar los 200 caracteres.");

        RuleFor(c => c.Cuerpo).NotEmpty().WithMessage("Ingresá el cuerpo del mensaje.");
    }
}
