using Esba.Domain.Certificados;

namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Contenido ya resuelto de la "Constancia de Materias Aprobadas" (CMA), listo para
/// que el servicio de reporte (QuestPDF) lo maquete. Toda la composición (párrafo de
/// encabezado, formateo de filas, fecha en letras) la hace el caso de uso: el servicio
/// de reporte solo dibuja la tabla (migration_improvements.md §2.1). Sucesor de
/// <c>BitBtn1Click</c> de constanciaalumnos2.pas.
/// </summary>
public sealed record ConstanciaMateriasAprobadasModel
{
    /// <summary>Párrafo de encabezado ("En Buenos Aires a los … perteneciente al alumno/a …").</summary>
    public required string Introduccion { get; init; }

    /// <summary>Filas del analítico ya formateadas y en orden por cuatrimestre.</summary>
    public required IReadOnlyList<FilaAnaliticoConstancia> Filas { get; init; }

    /// <summary>Destinatario ("Para ser presentada ante: …").</summary>
    public string? AnteQuien { get; init; }

    /// <summary>Nombre del instituto emisor (membrete).</summary>
    public string? Instituto { get; init; }

    /// <summary>Característica del instituto, p.ej. A-781 (membrete).</summary>
    public string? Caracteristica { get; init; }

    /// <summary>Nombre de la secretaria/o que firma.</summary>
    public string? Secretaria { get; init; }

    /// <summary>Nombre del rector/a que firma.</summary>
    public string? Rector { get; init; }
}
