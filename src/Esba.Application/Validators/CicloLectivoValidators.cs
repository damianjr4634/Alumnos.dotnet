using Esba.Application.DTOs.Academica;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Reglas de los ciclos lectivos (TBL_CUAT/TBL_TRIM). El legacy
/// (CargadeTrimestres.pas) grababa sin validar nada; acá se exige que cada
/// período tenga desde &lt; hasta y que los períodos no se superpongan.
/// </summary>
public sealed class GuardarCicloCuatrimestralValidator : AbstractValidator<GuardarCicloCuatrimestralCommand>
{
    public GuardarCicloCuatrimestralValidator()
    {
        RuleFor(c => c.Anio)
            .InclusiveBetween(1980, 2100).WithMessage("El año lectivo debe estar entre 1980 y 2100.");

        RuleFor(c => c.PrimerCuatrimestreDesde).NotNull().WithMessage("Ingrese el inicio del 1er cuatrimestre.");
        RuleFor(c => c.PrimerCuatrimestreHasta).NotNull().WithMessage("Ingrese el fin del 1er cuatrimestre.");
        RuleFor(c => c.SegundoCuatrimestreDesde).NotNull().WithMessage("Ingrese el inicio del 2do cuatrimestre.");
        RuleFor(c => c.SegundoCuatrimestreHasta).NotNull().WithMessage("Ingrese el fin del 2do cuatrimestre.");

        RuleFor(c => c)
            .Must(c => EnOrden(c.PrimerCuatrimestreDesde, c.PrimerCuatrimestreHasta))
            .WithMessage("El 1er cuatrimestre termina antes de empezar.")
            .Must(c => EnOrden(c.SegundoCuatrimestreDesde, c.SegundoCuatrimestreHasta))
            .WithMessage("El 2do cuatrimestre termina antes de empezar.")
            .Must(c => EnOrden(c.PrimerCuatrimestreHasta, c.SegundoCuatrimestreDesde))
            .WithMessage("El 2do cuatrimestre se superpone con el 1ro.");
    }

    /// <summary>true si falta alguna fecha (eso lo reporta el NotNull) o si van en orden estricto.</summary>
    internal static bool EnOrden(DateOnly? antes, DateOnly? despues) =>
        antes is null || despues is null || antes < despues;
}

public sealed class GuardarCicloTrimestralValidator : AbstractValidator<GuardarCicloTrimestralCommand>
{
    public GuardarCicloTrimestralValidator()
    {
        RuleFor(c => c.Anio)
            .InclusiveBetween(1980, 2100).WithMessage("El año lectivo debe estar entre 1980 y 2100.");

        RuleFor(c => c.PrimerTrimestreDesde).NotNull().WithMessage("Ingrese el inicio del 1er trimestre.");
        RuleFor(c => c.PrimerTrimestreHasta).NotNull().WithMessage("Ingrese el fin del 1er trimestre.");
        RuleFor(c => c.SegundoTrimestreDesde).NotNull().WithMessage("Ingrese el inicio del 2do trimestre.");
        RuleFor(c => c.SegundoTrimestreHasta).NotNull().WithMessage("Ingrese el fin del 2do trimestre.");
        RuleFor(c => c.TercerTrimestreDesde).NotNull().WithMessage("Ingrese el inicio del 3er trimestre.");
        RuleFor(c => c.TercerTrimestreHasta).NotNull().WithMessage("Ingrese el fin del 3er trimestre.");

        RuleFor(c => c)
            .Must(c => GuardarCicloCuatrimestralValidator.EnOrden(c.PrimerTrimestreDesde, c.PrimerTrimestreHasta))
            .WithMessage("El 1er trimestre termina antes de empezar.")
            .Must(c => GuardarCicloCuatrimestralValidator.EnOrden(c.SegundoTrimestreDesde, c.SegundoTrimestreHasta))
            .WithMessage("El 2do trimestre termina antes de empezar.")
            .Must(c => GuardarCicloCuatrimestralValidator.EnOrden(c.TercerTrimestreDesde, c.TercerTrimestreHasta))
            .WithMessage("El 3er trimestre termina antes de empezar.")
            .Must(c => GuardarCicloCuatrimestralValidator.EnOrden(c.PrimerTrimestreHasta, c.SegundoTrimestreDesde))
            .WithMessage("El 2do trimestre se superpone con el 1ro.")
            .Must(c => GuardarCicloCuatrimestralValidator.EnOrden(c.SegundoTrimestreHasta, c.TercerTrimestreDesde))
            .WithMessage("El 3er trimestre se superpone con el 2do.");
    }
}
