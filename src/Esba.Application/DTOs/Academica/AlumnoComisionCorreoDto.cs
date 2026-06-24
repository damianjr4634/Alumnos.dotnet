namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Alumno de una comisión para el envío de correo (sucesor del listado de
/// enviocorreo.pas, que separaba alumnos con y sin mail). La pantalla los divide
/// según <see cref="TieneMail"/>.
/// </summary>
public sealed record AlumnoComisionCorreoDto
{
    public required string CodigoAlumno { get; init; }

    public required string NombreCompleto { get; init; }

    /// <summary>MAIL del alumno; null/vacío si no tiene cargado.</summary>
    public string? Mail { get; init; }

    public bool TieneMail => !string.IsNullOrWhiteSpace(Mail);
}
