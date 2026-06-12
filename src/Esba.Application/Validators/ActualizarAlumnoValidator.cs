using Esba.Application.DTOs.Alumnos;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class ActualizarAlumnoValidator : AbstractValidator<ActualizarAlumnoCommand>
{
    public ActualizarAlumnoValidator()
    {
        RuleFor(c => c.Codigo).NotEmpty().Length(11)
            .WithMessage("Código de alumno inválido.");
        RuleFor(c => c.CodigoCarrera).NotEmpty().MaximumLength(6);

        RuleFor(c => c.Estado).IsInEnum().WithMessage("Seleccione un estado al alumno.");

        RuleFor(c => c.Apellido)
            .NotEmpty().WithMessage("Ingrese el apellido.")
            .MaximumLength(25).WithMessage("El apellido no puede superar los 25 caracteres.");
        RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("Ingrese el nombre.")
            .MaximumLength(25).WithMessage("El nombre no puede superar los 25 caracteres.");

        RuleFor(c => c.DocumentoExpedidoPor).MaximumLength(10);
        RuleFor(c => c.Nacionalidad).MaximumLength(15);
        RuleFor(c => c.LugarNacimiento).MaximumLength(15);
        RuleFor(c => c.ProvinciaNacimiento).MaximumLength(20);
        RuleFor(c => c.FechaNacimiento)
            .LessThan(DateOnly.FromDateTime(DateTime.Today)).WithMessage("La fecha de nacimiento debe ser anterior a hoy.")
            .When(c => c.FechaNacimiento.HasValue);

        RuleFor(c => c.Domicilio).MaximumLength(30);
        RuleFor(c => c.Localidad).MaximumLength(40);
        RuleFor(c => c.CodigoPostal).InclusiveBetween((short)1, (short)9999)
            .When(c => c.CodigoPostal.HasValue)
            .WithMessage("El código postal debe estar entre 1 y 9999.");
        RuleFor(c => c.CaracteristicaTelefono).MaximumLength(7);
        RuleFor(c => c.Telefono).MaximumLength(15);
        RuleFor(c => c.Celular).MaximumLength(20);
        RuleFor(c => c.Mail).MaximumLength(80)
            .EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Mail))
            .WithMessage("El e-mail no tiene un formato válido.");

        RuleFor(c => c.ColegioPrimario).MaximumLength(50);
        RuleFor(c => c.AnioPrimario).MaximumLength(2);
        RuleFor(c => c.TituloPrimario).MaximumLength(50);
        RuleFor(c => c.ColegioSecundario).MaximumLength(50);
        RuleFor(c => c.AnioSecundario).MaximumLength(2);
        RuleFor(c => c.TituloSecundario).MaximumLength(50);
        RuleFor(c => c.InstitucionTerciaria).MaximumLength(50);
        RuleFor(c => c.AnioTerciario).MaximumLength(2);
        RuleFor(c => c.TituloTerciario).MaximumLength(50);

        RuleFor(c => c.Empresa).MaximumLength(20);
        RuleFor(c => c.Rubro).MaximumLength(20);
        RuleFor(c => c.Cargo).MaximumLength(20);
        RuleFor(c => c.Antiguedad).MaximumLength(10);
        RuleFor(c => c.DomicilioLaboral).MaximumLength(25);
        RuleFor(c => c.TelefonoLaboral).MaximumLength(8);
        RuleFor(c => c.InternoLaboral).MaximumLength(5);

        RuleFor(c => c.UsuarioWeb).MaximumLength(20);
    }
}
