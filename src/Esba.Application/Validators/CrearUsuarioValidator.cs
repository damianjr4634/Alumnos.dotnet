using Esba.Application.DTOs.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class CrearUsuarioValidator : AbstractValidator<CrearUsuarioCommand>
{
    public CrearUsuarioValidator()
    {
        RuleFor(c => c.NombreUsuario)
            .NotEmpty().WithMessage("Ingrese el nombre de usuario.")
            .MaximumLength(15).WithMessage("El nombre de usuario no puede superar los 15 caracteres.");

        // El legacy limitaba el alta a 5 caracteres (absurdo); el hash PBKDF2 no
        // tiene tope técnico. Política nueva: mínimo 4 caracteres.
        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Ingrese la contraseña.")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres.");

        RuleFor(c => c.Nombres)
            .MaximumLength(50).WithMessage("Los nombres no pueden superar los 50 caracteres.");

        RuleFor(c => c.Apellido)
            .MaximumLength(50).WithMessage("El apellido no puede superar los 50 caracteres.");

        RuleFor(c => c.Cargo)
            .MaximumLength(30).WithMessage("El cargo no puede superar los 30 caracteres.");
    }
}
