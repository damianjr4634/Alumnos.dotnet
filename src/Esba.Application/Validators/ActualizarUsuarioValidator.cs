using Esba.Application.DTOs.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class ActualizarUsuarioValidator : AbstractValidator<ActualizarUsuarioCommand>
{
    public ActualizarUsuarioValidator()
    {
        RuleFor(c => c.Codigo)
            .GreaterThan(0).WithMessage("Usuario inválido.");

        RuleFor(c => c.NombreUsuario)
            .NotEmpty().WithMessage("Ingrese el nombre de usuario.")
            .MaximumLength(15).WithMessage("El nombre de usuario no puede superar los 15 caracteres.");

        RuleFor(c => c.Nombres)
            .MaximumLength(50).WithMessage("Los nombres no pueden superar los 50 caracteres.");

        RuleFor(c => c.Apellido)
            .MaximumLength(50).WithMessage("El apellido no puede superar los 50 caracteres.");

        RuleFor(c => c.Cargo)
            .MaximumLength(30).WithMessage("El cargo no puede superar los 30 caracteres.");
    }
}
