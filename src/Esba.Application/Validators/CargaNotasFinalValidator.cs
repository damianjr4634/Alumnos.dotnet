using Esba.Application.DTOs.Examenes;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Validación de la carga de notas de final. Réplica de los chequeos de la UI
/// legacy (FinalesxMesayComision.notaexit): las notas van en [1,10] o vacías
/// (los ausentes no se cargan), y si hay nota debe haber fecha.
/// </summary>
public sealed class CargaNotasFinalValidator : AbstractValidator<CargaNotasFinalCommand>
{
    public CargaNotasFinalValidator()
    {
        RuleFor(c => c.CodigoCarrera).NotEmpty().WithMessage("La carrera es obligatoria.");
        RuleFor(c => c.Mesa).GreaterThan(0).WithMessage("La mesa es obligatoria.");
        RuleFor(c => c.TipoCarrera).NotEmpty().WithMessage("Falta el tipo de carrera.");
        RuleFor(c => c.CodigoUsuario).GreaterThan(0).WithMessage("Usuario inválido.");
        RuleFor(c => c.Filas).NotEmpty().WithMessage("No hay notas para grabar.");

        RuleForEach(c => c.Filas).ChildRules(f =>
        {
            f.RuleFor(x => x.CodigoAlumno).NotEmpty().WithMessage("Falta el alumno de una fila.");
            f.RuleFor(x => x.CodigoMateria).NotEmpty().WithMessage("Falta la materia de una fila.");

            f.RuleFor(x => x.Nota1).Must(NotaValida)
                .WithMessage("La nota del 1° llamado debe estar entre 1 y 10 (los ausentes no se cargan).");
            f.RuleFor(x => x.Fecha1).NotNull().When(x => x.Nota1 is > 0)
                .WithMessage("Falta la fecha del 1° llamado.");

            f.RuleFor(x => x.Nota2).Must(NotaValida)
                .WithMessage("La nota del 2° llamado debe estar entre 1 y 10 (los ausentes no se cargan).");
            f.RuleFor(x => x.Fecha2).NotNull().When(x => x.Nota2 is > 0)
                .WithMessage("Falta la fecha del 2° llamado.");

            f.RuleFor(x => x.Nota3).Must(NotaValida)
                .WithMessage("La nota del 3° llamado debe estar entre 1 y 10 (los ausentes no se cargan).");
            f.RuleFor(x => x.Fecha3).NotNull().When(x => x.Nota3 is > 0)
                .WithMessage("Falta la fecha del 3° llamado.");
        });
    }

    private static bool NotaValida(decimal? nota) => nota is null || nota is >= 1 and <= 10;
}
