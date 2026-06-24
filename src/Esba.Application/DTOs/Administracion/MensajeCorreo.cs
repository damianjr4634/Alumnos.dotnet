namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Mensaje de correo a enviar. Contrato neutral de Application; el remitente (From)
/// lo fija la configuración institucional, no el mensaje (cuenta única, §2.7).
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

    /// <summary>Archivos adjuntos (sucesor de ArchAdj de enviocorreo.pas).</summary>
    public IReadOnlyList<AdjuntoCorreo> Adjuntos { get; init; } = [];
}

/// <summary>Archivo adjunto de un <see cref="MensajeCorreo"/>.</summary>
public sealed record AdjuntoCorreo
{
    public required string NombreArchivo { get; init; }

    public required byte[] Contenido { get; init; }

    /// <summary>Tipo MIME (ej. "application/pdf"). Por defecto binario genérico.</summary>
    public string TipoContenido { get; init; } = "application/octet-stream";
}
