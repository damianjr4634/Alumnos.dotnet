using Esba.Application.DTOs.Examenes;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Validación de los filtros del acta por comisión. Réplica de las exigencias de
/// lstactasARegular.pas (carrera del contexto + cuatrimestre obligatorio; comisión y
/// materia son opcionales).
/// </summary>
public sealed class GenerarActaComisionValidator : AbstractValidator<GenerarActaComisionCommand>
{
    public GenerarActaComisionValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CuatrimestreAnio).NotEmpty().WithMessage("El cuatrimestre es obligatorio.");
        RuleFor(c => c.Cutuco).GreaterThan((short)0).When(c => c.Cutuco.HasValue)
            .WithMessage("La comisión debe ser un número positivo.");
    }
}

/// <summary>
/// Validación de los filtros del acta volante de mesa. Réplica de lstactasMesas.pas
/// (la mesa es obligatoria; la carrera viene del contexto).
/// </summary>
public sealed class GenerarActaMesaValidator : AbstractValidator<GenerarActaMesaCommand>
{
    public GenerarActaMesaValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.Mesa).GreaterThan(0).WithMessage("El código de mesa es obligatorio.");
        RuleFor(c => c.TipoExamen).NotEmpty().WithMessage("El tipo de examen es obligatorio.");
    }
}
