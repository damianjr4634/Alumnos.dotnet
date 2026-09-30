namespace Esba.Application.DTOs.Ministerio;

/// <summary>
/// Datos de CARRERA que el padrón al Ministerio necesita para codificar sus columnas
/// (sucesor del LEFT JOIN CARRERA R de ComisionesAlMinisterio.pas y de la consulta a
/// Carreras.CarreIndexOf(VCarrera).tipo).
/// </summary>
public sealed record CarreraMinisterioDto
{
    public required string Codigo { get; init; }

    /// <summary>DESCARRE: nombre completo (encabezado de la nómina y orientación "TECNICATURA…").</summary>
    public string? Nombre { get; init; }

    /// <summary>TIPO: 'TER' usa el layout terciario; 'BAC' es modalidad común, el resto adultos.</summary>
    public string? Tipo { get; init; }

    /// <summary>RESOLUCION: número de resolución del plan.</summary>
    public string? Resolucion { get; init; }

    /// <summary>DISTANCIA='S': modalidad a distancia (turno 'TD').</summary>
    public bool EsADistancia { get; init; }
}
