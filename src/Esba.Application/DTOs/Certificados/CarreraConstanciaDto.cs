namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Datos de la carrera necesarios para emitir una constancia: autoridades que
/// firman (sucesoras de FuncionesConfiguracion.Rector/Secretaria, que en el legacy
/// se cargaban desde CARRERA), encabezado institucional e idioma extranjero del plan.
/// </summary>
public sealed record CarreraConstanciaDto
{
    /// <summary>DESCARRE: nombre completo de la carrera.</summary>
    public string? Nombre { get; init; }

    /// <summary>DESCARRE2: nombre del título intermedio (impresiones).</summary>
    public string? NombreAlternativo { get; init; }

    /// <summary>DURACION: duración del plan en texto.</summary>
    public string? Duracion { get; init; }

    /// <summary>RESOLUCION: resolución ministerial.</summary>
    public string? Resolucion { get; init; }

    /// <summary>RECTOR: nombre del rector/a que firma.</summary>
    public string? Rector { get; init; }

    /// <summary>SECRETARIA: nombre de la secretaria/o que firma.</summary>
    public string? Secretaria { get; init; }

    /// <summary>IDIOMA: idioma extranjero cursado (línea "* Idioma extranjero cursado: …").</summary>
    public string? Idioma { get; init; }

    /// <summary>INSTITUT: nombre del instituto emisor (membrete).</summary>
    public string? Instituto { get; init; }

    /// <summary>CARACT: característica del instituto, p.ej. A-781 (membrete).</summary>
    public string? Caracteristica { get; init; }

    /// <summary>true para carreras anuales (333/650): el listado de adeudadas dice "AÑO" en vez de "CUAT.".</summary>
    public bool EsCarreraPorAnio { get; init; }

    /// <summary>TIPO: tipo de carrera (TER/BAC/…). Define la línea de subvención de la constancia regular.</summary>
    public string? Tipo { get; init; }
}
