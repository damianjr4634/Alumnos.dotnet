using Esba.Application.DTOs.Examenes;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class CrearPermisoExamenValidator : AbstractValidator<CrearPermisoExamenCommand>
{
    public CrearPermisoExamenValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CodigoAlumno).NotEmpty().WithMessage("El alumno es obligatorio.");
        RuleFor(c => c.CodigoMateria).NotEmpty().WithMessage("La materia es obligatoria.");
        RuleFor(c => c.Mesa).GreaterThan(0).WithMessage("La mesa es obligatoria.");
        RuleFor(c => c.Cutuco).GreaterThan(0).WithMessage("La comisión es obligatoria.");
        RuleFor(c => c.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
    }
}
