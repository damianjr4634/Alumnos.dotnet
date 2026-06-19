using Esba.Application.DTOs.Academica;
using Esba.Domain.Enums;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class CrearEquivalenciaValidator : AbstractValidator<CrearEquivalenciaCommand>
{
    public CrearEquivalenciaValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CodigoAlumno).NotEmpty().WithMessage("El alumno es obligatorio.");
        RuleFor(c => c.CodigoMateria).NotEmpty().WithMessage("La materia es obligatoria.");

        RuleFor(c => c.InstitutoOrigen)
            .NotEmpty().WithMessage("La institución de origen es obligatoria.");

        // Para D.G.E.G.P. el número de actuación lo provee el operador; el interno lo
        // asigna el sistema (XXX_NUMERO_EQUIVALENCIA), por eso solo se exige acá.
        RuleFor(c => c.NumeroDgegp)
            .NotEmpty().WithMessage("El número de actuación D.G.E.G.P. es obligatorio.")
            .When(c => c.TipoActuacion == TipoActuacionEquivalencia.Dgegp);
    }
}
