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
            .NotEmpty().WithMessage("La institución de origen es obligatoria.")
            .MaximumLength(30).WithMessage("La institución de origen admite hasta 30 caracteres.");

        // Longitudes físicas de ANALITIC: FEQDOCE VARCHAR(3), FEQMATE VARCHAR(50), FEQCARRE VARCHAR(100).
        RuleFor(c => c.DocenteOrigen).MaximumLength(3).WithMessage("El código de docente admite hasta 3 caracteres.");
        RuleFor(c => c.MateriaOrigen).MaximumLength(50).WithMessage("La materia cursada en origen admite hasta 50 caracteres.");
        RuleFor(c => c.CarreraOrigen).MaximumLength(100).WithMessage("La carrera cursada en origen admite hasta 100 caracteres.");

        // Para D.G.E.G.P. el número de actuación lo provee el operador; el interno lo
        // asigna el sistema (XXX_NUMERO_EQUIVALENCIA), por eso solo se exige acá.
        RuleFor(c => c.NumeroDgegp)
            .NotEmpty().WithMessage("El número de actuación D.G.E.G.P. es obligatorio.")
            .When(c => c.TipoActuacion == TipoActuacionEquivalencia.Dgegp);
    }
}
