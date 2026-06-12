namespace Esba.Application.DTOs.Carreras;

/// <summary>
/// Carrera para listados y armado del menú por carrera (sucesor de
/// Carreras.pas / BarraCarrerasItems).
/// </summary>
public sealed record CarreraListItemDto
{
    public required string Codigo { get; init; }

    public string? Nombre { get; init; }

    public string? NombreCorto { get; init; }

    public required string Grupo { get; init; }

    public short? Orden { get; init; }

    public bool EsADistancia { get; init; }

    public bool Desactivada { get; init; }
}
