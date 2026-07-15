using Esba.Application.DTOs.Asistencias;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Validación de los filtros de las carpetas por comisión. Réplica de las exigencias
/// de lstplanasis.pas / lstNotasyPractico.pas (carrera del contexto + cuatrimestre
/// obligatorio; comisión y materia son opcionales).
/// </summary>
public sealed class GenerarCarpetaComisionValidator : AbstractValidator<GenerarCarpetaComisionCommand>
{
    public GenerarCarpetaComisionValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CuatrimestreAnio).NotEmpty().WithMessage("El cuatrimestre es obligatorio.");
        RuleFor(c => c.Cutuco).GreaterThan((short)0).When(c => c.Cutuco.HasValue)
            .WithMessage("La comisión debe ser un número positivo.");
    }
}
