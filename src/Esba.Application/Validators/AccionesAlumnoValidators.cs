using Esba.Application.DTOs.Alumnos;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>Copiar/mover alumno: alumno, carreras y usuario obligatorios; destino distinto del origen.</summary>
public sealed class CambiarCarreraAlumnoValidator : AbstractValidator<CambiarCarreraAlumnoCommand>
{
    public CambiarCarreraAlumnoValidator()
    {
        RuleFor(c => c.CodigoAlumno).NotEmpty().WithMessage("El alumno es obligatorio.");
        RuleFor(c => c.CodigoCarreraOrigen).NotEmpty().WithMessage("La carrera de origen es obligatoria.");
        RuleFor(c => c.CodigoCarreraDestino).NotEmpty().WithMessage("Elegí la carrera de destino.");
        RuleFor(c => c)
            .Must(c => !string.Equals(c.CodigoCarreraOrigen?.Trim(), c.CodigoCarreraDestino?.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("La carrera de destino debe ser distinta de la de origen.");
        RuleFor(c => c.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
    }
}

/// <summary>Borrar alumno: alumno, carrera y usuario obligatorios.</summary>
public sealed class BorrarAlumnoValidator : AbstractValidator<BorrarAlumnoCommand>
{
    public BorrarAlumnoValidator()
    {
        RuleFor(c => c.CodigoAlumno).NotEmpty().WithMessage("El alumno es obligatorio.");
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
    }
}
