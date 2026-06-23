namespace Esba.Application.DTOs.Academica;

/// <summary>Docente para combos y listados (sucesor del combo de cargacomisiones).</summary>
public sealed record DocenteListItemDto
{
    public required string Codigo { get; init; }

    public string? Nombre { get; init; }

    /// <summary>Documento (para la grilla del ABM); null en el combo simple.</summary>
    public string? NumeroDocumento { get; init; }

    public string? Localidad { get; init; }

    /// <summary>FECHA_BAJ: NULL = activo. Sólo lo trae el listado del ABM.</summary>
    public DateOnly? FechaBaja { get; init; }

    public bool EstaDeBaja => FechaBaja is not null;
}
