using Esba.Application.DTOs.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class BlanquearPasswordValidator : AbstractValidator<BlanquearPasswordCommand>
{
    public BlanquearPasswordValidator()
    {
        RuleFor(c => c.CodigoUsuario)
            .GreaterThan(0).WithMessage("Usuario inválido.");

        RuleFor(c => c.PasswordTemporal)
            .NotEmpty().WithMessage("Ingrese la contraseña temporal.")
            .MinimumLength(4).WithMessage("La contraseña temporal debe tener al menos 4 caracteres.");
    }
}
