namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Envío de correo a los alumnos de una comisión (sucesor de enviocorreo.pas). Las
/// direcciones ya vienen resueltas desde la pantalla (alumnos seleccionados + manuales).
/// Se envía un único mensaje (no uno por alumno) más una copia interna de auditoría.
/// </summary>
public sealed record EnviarCorreoComisionCommand
{
    public IReadOnlyList<string> Para { get; init; } = [];

    public IReadOnlyList<string> CopiaCarbon { get; init; } = [];

    public IReadOnlyList<string> CopiaOculta { get; init; } = [];

    public required string Asunto { get; init; }

    public required string Cuerpo { get; init; }

    /// <summary>Nombre del usuario que envía, para la copia de auditoría.</summary>
    public string? UsuarioRemitente { get; init; }
}
