using Esba.Application.DTOs.Academica;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Validación de la regularización de bachillerato. Réplica de los chequeos de la UI
/// legacy: las notas del cursado (2 bimestres, recuperatorio y nota a-regular) van en
/// [1,10], vacías, o el centinela 99 (ausente); las faltas no son negativas.
/// </summary>
public sealed class ConfirmarRegularizacionBachilleratoValidator : AbstractValidator<ConfirmarRegularizacionBachilleratoCommand>
{
    public ConfirmarRegularizacionBachilleratoValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
        RuleFor(c => c.Filas).NotEmpty().WithMessage("No hay materias para regularizar.");

        RuleForEach(c => c.Filas).ChildRules(f =>
        {
            f.RuleFor(x => x.CodigoAlumno).NotEmpty().WithMessage("Falta el alumno de una fila.");
            f.RuleFor(x => x.CodigoMateria).NotEmpty().WithMessage("Falta la materia de una fila.");
            f.RuleFor(x => x.CuatrimestreAnio).NotEmpty().WithMessage("Falta el cuatrimestre de una fila.");

            f.RuleFor(x => x.TpEva).Must(NotaValida).WithMessage("El 1° bimestre debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.TpEva2).Must(NotaValida).WithMessage("El 2° bimestre debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.Recuperatorio).Must(NotaValida).WithMessage("El recuperatorio debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.NotaRegular).Must(NotaValida).WithMessage("La nota a regular debe estar entre 1 y 10 (o 99 = ausente).");

            f.RuleFor(x => x.TotalHoras).GreaterThanOrEqualTo((short)0).When(x => x.TotalHoras.HasValue)
                .WithMessage("Las horas no pueden ser negativas.");
            f.RuleFor(x => x.Inasistencias).GreaterThanOrEqualTo((short)0).When(x => x.Inasistencias.HasValue)
                .WithMessage("Las inasistencias no pueden ser negativas.");
            f.RuleFor(x => x.Justificadas).GreaterThanOrEqualTo((short)0).When(x => x.Justificadas.HasValue)
                .WithMessage("Las justificadas no pueden ser negativas.");
        });
    }

    // Nota válida: vacía, en [1,10], o el centinela 99 (ausente/no rendido).
    private static bool NotaValida(decimal? nota) => nota is null || nota == 99m || nota is >= 1m and <= 10m;
}
