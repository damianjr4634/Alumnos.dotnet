using Esba.Application.DTOs.Asistencias;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class GuardarInasistenciasComisionValidator : AbstractValidator<GuardarInasistenciasComisionCommand>
{
    public GuardarInasistenciasComisionValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");

        RuleFor(c => c.Cutuco)
            .InclusiveBetween((short)100, (short)999)
            .WithMessage("La comisión (CUTUCO) debe tener 3 dígitos.");

        RuleFor(c => c.CuatrimestreAnio)
            .NotEmpty().WithMessage("El cuatrimestre/año es obligatorio.")
            .Must(c => c is not null && c.Trim().Length == 3)
            .WithMessage("El cuatrimestre/año debe tener 3 dígitos (ej. 124).");

        RuleFor(c => c.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");

        RuleForEach(c => c.Faltas).ChildRules(f =>
        {
            f.RuleFor(x => x.CodigoAlumno).NotEmpty().WithMessage("Falta el alumno de una inasistencia.");
            f.RuleFor(x => x.CodigoFalta).NotEmpty().WithMessage("Falta el tipo de una inasistencia.");
        });
    }
}
