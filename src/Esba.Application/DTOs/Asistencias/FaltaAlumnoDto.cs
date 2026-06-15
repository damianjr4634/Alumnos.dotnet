namespace Esba.Application.DTOs.Asistencias;

/// <summary>Fila de XXX_FALTAS_FALTAS: una falta cargada de un alumno (para precargar el calendario).</summary>
public sealed record FaltaAlumnoDto
{
    public DateOnly Fecha { get; init; }

    public required string CodigoFalta { get; init; }

    public double Cantidad { get; init; }

    public string? Descripcion { get; init; }
}
