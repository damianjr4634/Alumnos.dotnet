using Esba.Application.DTOs.Ministerio;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Validación de los filtros de la nómina impresa para el Ministerio. Réplica de las
/// exigencias de ComisionesAlMinisterio.pas (carrera del contexto + cuatrimestre
/// obligatorio; comisión opcional; margen superior razonable para una hoja A4).
/// </summary>
public sealed class GenerarNominaMinisterioValidator : AbstractValidator<GenerarNominaMinisterioCommand>
{
    public const decimal MargenSuperiorMaximoCm = 20m;

    public GenerarNominaMinisterioValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CuatrimestreAnio).NotEmpty().WithMessage("El cuatrimestre es obligatorio.");
        RuleFor(c => c.Cutuco).GreaterThan((short)0).When(c => c.Cutuco.HasValue)
            .WithMessage("La comisión debe ser un número positivo.");
        RuleFor(c => c.MargenSuperiorCm).InclusiveBetween(0m, MargenSuperiorMaximoCm)
            .WithMessage($"El margen superior debe estar entre 0 y {MargenSuperiorMaximoCm} cm.");
    }
}

/// <summary>Validación de los filtros del export Excel del padrón al Ministerio.</summary>
public sealed class ExportarPadronMinisterioValidator : AbstractValidator<ExportarPadronMinisterioCommand>
{
    public ExportarPadronMinisterioValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CuatrimestreAnio).NotEmpty().WithMessage("El cuatrimestre es obligatorio.");
        RuleFor(c => c.Cutuco).GreaterThan((short)0).When(c => c.Cutuco.HasValue)
            .WithMessage("La comisión debe ser un número positivo.");
    }
}
