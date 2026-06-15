namespace Esba.Application.DTOs.Examenes;

/// <summary>Tipo de mesa para el combo del ABM (MESA_TIPO filtrado por tipo de carrera).</summary>
public sealed record TipoMesaDto
{
    public required string Codigo { get; init; }

    public required string Descripcion { get; init; }
}
