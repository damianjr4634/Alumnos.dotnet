namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Próximo número de actuación de equivalencia sugerido por <c>XXX_NUMERO_EQUIVALENCIA</c>.
/// El SP devuelve el último número del alumno (si ya tiene equivalencias este año) o
/// el siguiente disponible de la carrera (numeración nueva).
/// </summary>
public sealed record NumeroEquivalenciaDto
{
    /// <summary>NUM_FORMA: número formateado para mostrar/editar (p.ej. "0000000000123/24").</summary>
    public required string NumeroFormateado { get; init; }

    /// <summary>NUM_ENTERO: el mismo número sin separador (lo que termina en ACTINT, 15 dígitos).</summary>
    public required string NumeroEntero { get; init; }

    /// <summary>FERRMSG: mensaje informativo del SP (p.ej. "tiene dos números…"), o vacío.</summary>
    public string? Mensaje { get; init; }

    /// <summary>FNUMNUE='S': el número es nuevo (recién tomado de la carrera) y, si se usa, hay que confirmarlo con XXX_GRABA_NUMEQUI.</summary>
    public bool EsNuevo { get; init; }
}
