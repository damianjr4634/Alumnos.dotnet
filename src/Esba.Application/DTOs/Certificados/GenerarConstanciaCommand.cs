using Esba.Domain.Enums;

namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Solicitud de emisión de una constancia de alumno (sucesora de los botones de
/// constanciaalumnos2.pas). El "ante quién" lo tipea el operador en el formulario.
/// </summary>
public sealed record GenerarConstanciaCommand
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    public required TipoConstancia Tipo { get; init; }

    /// <summary>Destinatario de la constancia ("para ser presentado ante: …").</summary>
    public string? AnteQuien { get; init; }
}
