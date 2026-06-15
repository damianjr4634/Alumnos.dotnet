namespace Esba.Application.DTOs.Asistencias;

/// <summary>Tipo de inasistencia para el combo de carga (sucesor del lookup a TBL_FALTAS).</summary>
public sealed record TipoFaltaDto
{
    public required string Codigo { get; init; }

    public string? Descripcion { get; init; }

    public decimal Cantidad { get; init; }

    public bool Justifica { get; init; }
}
