using Esba.Application.DTOs.AreaDocente;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Precarga de notas de final: misma regla de nota que la carga de finales de secretaría
/// (CargaNotasFinalValidator: vacía o en [1,10]); "ausente" es una marca aparte y no
/// convive con una nota.
/// </summary>
public sealed class GuardarCargaMesaValidator : AbstractValidator<GuardarCargaMesaCommand>
{
    public GuardarCargaMesaValidator()
    {
        RuleFor(c => c.Mesa).NotNull().WithMessage("Falta la mesa.");
        RuleFor(c => c.Mesa.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.Mesa.NumeroMesa).GreaterThan(0).WithMessage("Mesa inválida.");
        RuleFor(c => c.Actor.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
        RuleFor(c => c.Observaciones).MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres.");

        RuleForEach(c => c.Filas).ChildRules(f =>
        {
            f.RuleFor(x => x.CodigoAlumno).NotEmpty().WithMessage("Falta el alumno de una fila.");
            f.RuleFor(x => x.CodigoMateria).NotEmpty().WithMessage("Falta la materia de una fila.");
            f.RuleFor(x => x.Nota).Must(nota => nota is null || nota is >= 1m and <= 10m)
                .WithMessage("La nota de final debe estar entre 1 y 10.");
            f.RuleFor(x => x.Nota).Null().When(x => x.Ausente)
                .WithMessage("Un alumno ausente no lleva nota.");
            f.RuleFor(x => x.Observaciones).MaximumLength(500)
                .WithMessage("La observación de un alumno no puede superar los 500 caracteres.");
        });
    }
}
