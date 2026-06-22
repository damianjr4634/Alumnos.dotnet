using Esba.Application.DTOs.Administracion;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class AsignarPermisosUsuarioValidator : AbstractValidator<AsignarPermisosUsuarioCommand>
{
    public AsignarPermisosUsuarioValidator()
    {
        RuleFor(c => c.CodigoUsuario)
            .GreaterThan(0).WithMessage("Usuario inválido.");

        // La lista puede ir vacía (quita todos los permisos), pero no nula.
        RuleFor(c => c.CodigosOpcion)
            .NotNull().WithMessage("Lista de permisos inválida.");
    }
}
