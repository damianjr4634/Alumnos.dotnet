using Esba.Application.Features.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class IniciarSesionValidator : AbstractValidator<IniciarSesionCommand>
{
    public IniciarSesionValidator()
    {
        RuleFor(c => c.NombreUsuario)
            .NotEmpty().WithMessage("Ingrese el nombre de usuario.")
            .MaximumLength(15).WithMessage("El nombre de usuario no puede superar los 15 caracteres.");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Ingrese la contraseña.");
    }
}
