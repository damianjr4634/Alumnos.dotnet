using Esba.Application.DTOs.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class EnviarCorreoPruebaValidator : AbstractValidator<EnviarCorreoPruebaCommand>
{
    public EnviarCorreoPruebaValidator()
    {
        RuleFor(c => c.Destinatario)
            .NotEmpty().WithMessage("Ingrese una dirección de correo de destino.")
            .EmailAddress().WithMessage("La dirección de correo no es válida.");
    }
}
