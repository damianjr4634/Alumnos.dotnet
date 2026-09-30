using Esba.Application.DTOs.Examenes;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Validación de la citación a profesores. El legacy exigía las fechas ('S' en el panel
/// de parámetros) y aplicaba el rango de docentes solo si venían ambos extremos;
/// acá un extremo suelto se rechaza en vez de ignorarse en silencio.
/// </summary>
public sealed class GenerarCitacionDocentesValidator : AbstractValidator<GenerarCitacionDocentesCommand>
{
    public GenerarCitacionDocentesValidator()
    {
        RuleFor(c => c.FechaHasta).GreaterThanOrEqualTo(c => c.FechaDesde)
            .WithMessage("La fecha hasta no puede ser anterior a la fecha desde.");
        RuleFor(c => c)
            .Must(c => string.IsNullOrWhiteSpace(c.CodigoProfesorDesde) == string.IsNullOrWhiteSpace(c.CodigoProfesorHasta))
            .WithMessage("Indicá docente desde y hasta, o ninguno de los dos.");
        RuleFor(c => c.CodigoProfesorHasta)
            .Must((c, hasta) => string.IsNullOrWhiteSpace(hasta)
                || string.CompareOrdinal(c.CodigoProfesorDesde?.Trim(), hasta.Trim()) <= 0)
            .WithMessage("El docente hasta no puede ser anterior al docente desde.");
        RuleFor(c => c.CodigoCarreraFirma).NotEmpty()
            .WithMessage("Elegí la carrera cuyas autoridades firman la citación.");
    }
}

/// <summary>Validación del parte diario de mesas: rango de fechas coherente.</summary>
public sealed class GenerarParteDiarioMesasValidator : AbstractValidator<GenerarParteDiarioMesasCommand>
{
    public GenerarParteDiarioMesasValidator()
    {
        RuleFor(c => c.FechaHasta).GreaterThanOrEqualTo(c => c.FechaDesde)
            .WithMessage("La fecha hasta no puede ser anterior a la fecha desde.");
    }
}
