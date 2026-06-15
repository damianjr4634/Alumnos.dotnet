using Esba.Application.DTOs.Academica;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class InscribirCuatrimestreCompletoValidator : AbstractValidator<InscribirCuatrimestreCompletoCommand>
{
    public InscribirCuatrimestreCompletoValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CodigoAlumno).NotEmpty().WithMessage("El alumno es obligatorio.");

        RuleFor(c => c.Curso)
            .InclusiveBetween((short)100, (short)999)
            .WithMessage("El curso (CUTUCO) debe tener 3 dígitos.");

        RuleFor(c => c.CuatrimestreAnio)
            .NotEmpty().WithMessage("El cuatrimestre/año es obligatorio.")
            .Must(c => c is not null && c.Trim().Length == 3)
            .WithMessage("El cuatrimestre/año debe tener 3 dígitos (ej. 124).");

        RuleFor(c => c.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
    }
}
