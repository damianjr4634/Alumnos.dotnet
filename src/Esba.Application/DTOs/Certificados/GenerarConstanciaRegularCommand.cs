namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Solicitud de Constancia de Alumno Regular. Sucesor de los parámetros del
/// formulario constanciaalumnoregular.pas (alumno, carrera, "ante quién").
/// </summary>
public sealed record GenerarConstanciaRegularCommand
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    /// <summary>Ante quién se presenta la constancia.</summary>
    public string? AnteQuien { get; init; }

    /// <summary>Si se compone el membrete institucional de fondo.</summary>
    public bool IncluirMembrete { get; init; } = true;
}
