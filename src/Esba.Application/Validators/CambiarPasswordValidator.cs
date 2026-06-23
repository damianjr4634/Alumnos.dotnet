using Esba.Application.DTOs.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class CambiarPasswordValidator : AbstractValidator<CambiarPasswordCommand>
{
    public CambiarPasswordValidator()
    {
        RuleFor(c => c.PasswordActual)
            .NotEmpty().WithMessage("Ingrese la contraseña actual.");

        RuleFor(c => c.PasswordNueva)
            .NotEmpty().WithMessage("Ingrese la nueva contraseña.")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres.")
            .NotEqual(c => c.PasswordActual).WithMessage("La nueva contraseña debe ser distinta de la actual.");

        RuleFor(c => c.PasswordNuevaConfirmacion)
            .Equal(c => c.PasswordNueva).WithMessage("Las contraseñas no coinciden.");
    }
}
