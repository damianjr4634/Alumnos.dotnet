namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Contenido ya resuelto de la Constancia de Alumno Regular, listo para que el
/// servicio de reporte (QuestPDF) lo maquete. La composición de texto la hace el
/// caso de uso vía el formatter de dominio; el servicio solo dibuja (§2.1).
/// </summary>
public sealed record ConstanciaRegularModel
{
    /// <summary>Título centrado del documento.</summary>
    public required string Titulo { get; init; }

    /// <summary>Párrafos del cuerpo, en orden.</summary>
    public required IReadOnlyList<string> Cuerpo { get; init; }

    /// <summary>Nota legal al pie.</summary>
    public required string NotaLegal { get; init; }

    /// <summary>Línea de subvención del Estado (TER/BAC) o null.</summary>
    public string? LineaSubvencion { get; init; }

    /// <summary>Nombre de la secretaria/o que firma.</summary>
    public string? Secretaria { get; init; }

    /// <summary>Nombre del rector/a que firma.</summary>
    public string? Rector { get; init; }
}
