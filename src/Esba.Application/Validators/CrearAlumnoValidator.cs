using Esba.Application.DTOs.Alumnos;
using FluentValidation;

namespace Esba.Application.Validators;

/// <summary>
/// Validación del alta de alumno. El legacy validaba poco en el formulario
/// (estado obligatorio, documento numérico); acá se agregan las longitudes del
/// DDL y formatos básicos como barrera previa a la base (§2.4: la violación de
/// constraint es el último recurso, no el mecanismo principal).
/// </summary>
public sealed class CrearAlumnoValidator : AbstractValidator<CrearAlumnoCommand>
{
    private static readonly string[] TiposDocumento = ["DNI", "CI", "LE", "PAS"];

    public CrearAlumnoValidator()
    {
        RuleFor(c => c.TipoDocumento)
            .NotEmpty().WithMessage("Seleccione el tipo de documento.")
            .Must(t => TiposDocumento.Contains(t.Trim().ToUpperInvariant()))
            .WithMessage("Tipo de documento inválido (DNI, CI, LE o PAS).");

        RuleFor(c => c.NumeroDocumento)
            .NotEmpty().WithMessage("Ingrese el número de documento.")
            .Matches("^[0-9]{1,8}$").WithMessage("El documento debe ser numérico, de hasta 8 dígitos.");

        RuleFor(c => c.CodigoCarrera)
            .NotEmpty().WithMessage("Seleccione la carrera.")
            .MaximumLength(6);

        RuleFor(c => c.Estado).IsInEnum().WithMessage("Seleccione un estado al alumno.");

        AplicarReglasDeDatos();
    }

    private void AplicarReglasDeDatos()
    {
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
