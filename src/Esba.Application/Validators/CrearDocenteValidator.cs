using Esba.Application.DTOs.Academica;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class CrearDocenteValidator : AbstractValidator<CrearDocenteCommand>
{
    public CrearDocenteValidator()
    {
        RuleFor(c => c.Codigo)
            .NotEmpty().WithMessage("Ingrese el código del docente.")
            .MaximumLength(3).WithMessage("El código no puede superar los 3 caracteres.");

        RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("Ingrese el apellido y nombre.")
            .MaximumLength(80).WithMessage("El nombre no puede superar los 80 caracteres.");

        RuleFor(c => c.TipoDocumento).MaximumLength(3);
        RuleFor(c => c.NumeroDocumento).MaximumLength(8);
        RuleFor(c => c.Direccion).MaximumLength(30);
        RuleFor(c => c.Piso).MaximumLength(2);
        RuleFor(c => c.Departamento).MaximumLength(2);
        RuleFor(c => c.CodigoPostal).MaximumLength(4);
        RuleFor(c => c.Localidad).MaximumLength(30);
        RuleFor(c => c.TelefonoParticular).MaximumLength(20);
        RuleFor(c => c.TelefonoMensajes).MaximumLength(20);
        RuleFor(c => c.Interno).MaximumLength(4);
    }
}
