namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Mensaje de correo a enviar. Contrato neutral de Application; el remitente (From)
/// lo fija la configuración institucional, no el mensaje (cuenta única, §2.7).
/// El envío con adjuntos y por comisión se construye sobre esto en el hito 10.4.
/// </summary>
public sealed record MensajeCorreo
{
    public required IReadOnlyList<string> Para { get; init; }

    public IReadOnlyList<string> CopiaCarbon { get; init; } = [];

    public IReadOnlyList<string> CopiaOculta { get; init; } = [];

    public required string Asunto { get; init; }

    public required string Cuerpo { get; init; }

    /// <summary>true si el cuerpo es HTML; false = texto plano.</summary>
    public bool EsHtml { get; init; }
}
