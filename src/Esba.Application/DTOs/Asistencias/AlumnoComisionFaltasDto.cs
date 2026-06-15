namespace Esba.Application.DTOs.Asistencias;

/// <summary>
/// Fila de XXX_FALTAS_COMISION: alumno de la comisión con su acumulado de faltas.
/// La carga de inasistencias lista los alumnos con esto.
/// </summary>
public sealed record AlumnoComisionFaltasDto
{
    public required string CodigoAlumno { get; init; }

    public string? Nombre { get; init; }

    /// <summary>CANANT: cantidad de faltas acumuladas (anteriores).</summary>
    public double CantidadAnterior { get; init; }

    /// <summary>CODFAL/CANTID: última falta computada (según el SP).</summary>
    public string? CodigoFalta { get; init; }

    public double Cantidad { get; init; }
}
