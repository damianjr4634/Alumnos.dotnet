using Esba.Application.DTOs.Certificados;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class GenerarConstanciaCommandValidator : AbstractValidator<GenerarConstanciaCommand>
{
    public GenerarConstanciaCommandValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CodigoAlumno).NotEmpty().WithMessage("El alumno es obligatorio.");
        RuleFor(c => c.AnteQuien)
            .NotEmpty().WithMessage("Indique ante quién se presenta la constancia.")
            .MaximumLength(150).WithMessage("El destinatario no puede superar los 150 caracteres.");
    }
}
