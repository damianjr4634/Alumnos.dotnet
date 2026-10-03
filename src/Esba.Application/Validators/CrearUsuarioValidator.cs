using Esba.Application.DTOs.Administracion;
using Esba.Domain.Enums;
using FluentValidation;

namespace Esba.Application.Validators;

public sealed class CrearUsuarioValidator : AbstractValidator<CrearUsuarioCommand>
{
    public CrearUsuarioValidator()
    {
        RuleFor(c => c.NombreUsuario)
            .NotEmpty().WithMessage("Ingrese el nombre de usuario.")
            .MaximumLength(15).WithMessage("El nombre de usuario no puede superar los 15 caracteres.");

        // El legacy limitaba el alta a 5 caracteres (absurdo); el hash PBKDF2 no
        // tiene tope técnico. Política nueva: mínimo 4 caracteres.
        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Ingrese la contraseña.")
            .MinimumLength(4).WithMessage("La contraseña debe tener al menos 4 caracteres.");

        RuleFor(c => c.Nombres)
            .MaximumLength(50).WithMessage("Los nombres no pueden superar los 50 caracteres.");

        RuleFor(c => c.Apellido)
            .MaximumLength(50).WithMessage("El apellido no puede superar los 50 caracteres.");

        RuleFor(c => c.Cargo)
            .MaximumLength(30).WithMessage("El cargo no puede superar los 30 caracteres.");

        Include(new TipoYVinculoUsuarioRules<CrearUsuarioCommand>(
            c => c.Tipo, c => c.EsSupervisor, c => c.CodigoDocente, c => c.AlumnoCarrera, c => c.AlumnoCodigo));
    }
}

/// <summary>
/// Reglas de tipo de usuario y vínculo compartidas por alta y modificación
/// (regla de los tres lugares, §2.1.4): el vínculo obligatorio depende del tipo y
/// solo secretaría puede ser supervisor. Sin FK en la base: esta es la barrera.
/// </summary>
internal sealed class TipoYVinculoUsuarioRules<T> : AbstractValidator<T>
{
    public TipoYVinculoUsuarioRules(
        Func<T, TipoUsuario> tipo,
        Func<T, bool> esSupervisor,
        Func<T, string?> codigoDocente,
        Func<T, string?> alumnoCarrera,
        Func<T, string?> alumnoCodigo)
    {
        RuleFor(c => tipo(c))
            .IsInEnum().WithMessage("Tipo de usuario inválido.")
            .OverridePropertyName("Tipo");

        RuleFor(c => esSupervisor(c))
            .Equal(false)
            .When(c => tipo(c) != TipoUsuario.Secretaria)
            .WithMessage("Solo un usuario de secretaría puede ser supervisor.")
            .OverridePropertyName("EsSupervisor");

        RuleFor(c => codigoDocente(c))
            .NotEmpty().WithMessage("Seleccione el docente al que queda vinculado el usuario.")
            .MaximumLength(3).WithMessage("El código de docente no puede superar los 3 caracteres.")
            .When(c => tipo(c) == TipoUsuario.Docente)
            .OverridePropertyName("CodigoDocente");

        RuleFor(c => alumnoCarrera(c))
            .NotEmpty().WithMessage("Indique la carrera del alumno vinculado.")
            .MaximumLength(6).WithMessage("El código de carrera no puede superar los 6 caracteres.")
            .When(c => tipo(c) == TipoUsuario.Alumno)
            .OverridePropertyName("AlumnoCarrera");

        RuleFor(c => alumnoCodigo(c))
            .NotEmpty().WithMessage("Indique el código del alumno vinculado.")
            .MaximumLength(11).WithMessage("El código de alumno no puede superar los 11 caracteres.")
            .When(c => tipo(c) == TipoUsuario.Alumno)
            .OverridePropertyName("AlumnoCodigo");
    }
}
