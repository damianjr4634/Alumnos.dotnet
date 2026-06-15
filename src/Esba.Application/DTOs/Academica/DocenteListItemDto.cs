namespace Esba.Application.DTOs.Academica;

/// <summary>Docente para combos y listados (sucesor del combo de cargacomisiones).</summary>
public sealed record DocenteListItemDto
{
    public required string Codigo { get; init; }

    public string? Nombre { get; init; }
}
