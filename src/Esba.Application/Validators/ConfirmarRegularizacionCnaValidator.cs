using Esba.Application.DTOs.Academica;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Validación de la regularización de CNA: la nota final va entre 0 y 10, y la fecha del
/// examen es obligatoria (el formulario legacy la exige antes de grabar).
/// </summary>
public sealed class ConfirmarRegularizacionCnaValidator : AbstractValidator<ConfirmarRegularizacionCnaCommand>
{
    public ConfirmarRegularizacionCnaValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
        RuleFor(c => c.Filas).NotEmpty().WithMessage("No hay materias para regularizar.");

        RuleForEach(c => c.Filas).ChildRules(f =>
        {
            f.RuleFor(x => x.CodigoAlumno).NotEmpty().WithMessage("Falta el alumno de una fila.");
            f.RuleFor(x => x.CodigoMateria).NotEmpty().WithMessage("Falta la materia de una fila.");
            f.RuleFor(x => x.CuatrimestreAnio).NotEmpty().WithMessage("Falta el cuatrimestre de una fila.");

            f.RuleFor(x => x.NotaFinal).InclusiveBetween(0m, 10m).When(x => x.NotaFinal.HasValue)
                .WithMessage("La nota final debe estar entre 0 y 10.");
            f.RuleFor(x => x.Fecha).NotNull().WithMessage("La fecha del examen final es obligatoria.");
        });
    }
}
