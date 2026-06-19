namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Contenido ya resuelto de una constancia de alumno, listo para que el servicio
/// de reporte (QuestPDF) lo maquete. Toda la composición de texto (párrafo,
/// materias que adeuda, párrafo de cierre con fecha en letras) la hace el caso de
/// uso: el servicio de reporte solo dibuja (migration_improvements.md §2.1).
/// </summary>
public sealed record ConstanciaAlumnoModel
{
    /// <summary>Título centrado del documento.</summary>
    public required string Titulo { get; init; }

    /// <summary>Cuerpo principal (FPARRAFO de XXX_PARRAFO_CONSTANCIA).</summary>
    public required string Parrafo { get; init; }

    /// <summary>
    /// Línea "* Materias que adeuda: …". Null/vacío en constancias que no la llevan
    /// (p.ej. la de examen final): en ese caso no se imprime el bloque "DATOS CORRESPONDIENTES".
    /// </summary>
    public string? MateriasQueAdeuda { get; init; }

    /// <summary>Línea "* Idioma extranjero cursado: …" o null si la carrera no tiene idioma.</summary>
    public string? IdiomaLinea { get; init; }

    /// <summary>Párrafo de cierre ("A pedido del interesado… ante: …", con fecha en letras).</summary>
    public required string ParrafoCierre { get; init; }

    /// <summary>Notas legales al pie.</summary>
    public required IReadOnlyList<string> NotasLegales { get; init; }

    /// <summary>Nombre de la secretaria/o que firma (puede faltar en la carrera).</summary>
    public string? Secretaria { get; init; }

    /// <summary>Nombre del rector/a que firma.</summary>
    public string? Rector { get; init; }

    /// <summary>Si se compone el membrete institucional.</summary>
    public bool IncluirMembrete { get; init; }

    /// <summary>Nombre del instituto emisor (membrete).</summary>
    public string? Instituto { get; init; }

    /// <summary>Característica del instituto, p.ej. A-781 (membrete).</summary>
    public string? Caracteristica { get; init; }

    /// <summary>Nombre de la carrera (membrete/subtítulo).</summary>
    public string? NombreCarrera { get; init; }
}
