using Esba.Application.DTOs.AreaDocente;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Precarga del docente: mismas reglas que la regularización (las notas en [1,10],
/// vacías o el centinela 99 = ausente; totales no negativos), para que lo que el
/// docente deja listo pase la validación de secretaría al efectivizar.
/// </summary>
public sealed class GuardarCargaComisionValidator : AbstractValidator<GuardarCargaComisionCommand>
{
    public GuardarCargaComisionValidator()
    {
        RuleFor(c => c.Comision).NotNull().WithMessage("Falta la comisión.");
        RuleFor(c => c.Comision.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.Comision.Cutuco).GreaterThan((short)0).WithMessage("Comisión inválida.");
        RuleFor(c => c.Comision.CodigoMateria).NotEmpty().WithMessage("La materia es obligatoria.");
        RuleFor(c => c.Comision.CuatrimestreAnio).NotEmpty().WithMessage("El cuatrimestre es obligatorio.");
        RuleFor(c => c.Actor.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
        RuleFor(c => c.Observaciones).MaximumLength(500).WithMessage("Las observaciones no pueden superar los 500 caracteres.");

        RuleForEach(c => c.Filas).ChildRules(f =>
        {
            f.RuleFor(x => x.CodigoAlumno).NotEmpty().WithMessage("Falta el alumno de una fila.");
            f.RuleFor(x => x.Evaluacion1).Must(NotaValida).WithMessage("La 1° evaluación debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.Evaluacion2).Must(NotaValida).WithMessage("La 2° evaluación debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.Evaluacion3).Must(NotaValida).WithMessage("La 3° evaluación debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.Recuperatorio).Must(NotaValida).WithMessage("El recuperatorio debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.NotaRegular).Must(NotaValida).WithMessage("La nota a regularizar debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.NotaFinal).Must(NotaValida).WithMessage("La nota final debe estar entre 1 y 10 (o 99 = ausente).");
            f.RuleFor(x => x.TotalHoras).GreaterThanOrEqualTo((short)0).When(x => x.TotalHoras.HasValue)
                .WithMessage("Las horas no pueden ser negativas.");
            f.RuleFor(x => x.Inasistencias).GreaterThanOrEqualTo((short)0).When(x => x.Inasistencias.HasValue)
                .WithMessage("Las inasistencias no pueden ser negativas.");
            f.RuleFor(x => x.Justificadas).GreaterThanOrEqualTo((short)0).When(x => x.Justificadas.HasValue)
                .WithMessage("Las justificadas no pueden ser negativas.");
            f.RuleFor(x => x.Observaciones).MaximumLength(500)
                .WithMessage("La observación de un alumno no puede superar los 500 caracteres.");
        });
    }

    // Nota válida: vacía, en [1,10], o el centinela 99 (ausente/no rendido). Igual que ConfirmarRegularizacionValidator.
    private static bool NotaValida(decimal? nota) => nota is null || nota == 99m || nota is >= 1m and <= 10m;
}
