namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Fila de regularización de una materia de <b>CNA</b>: solo nota final + fecha. Sucesora de
/// la carga a "$$$CURSADA" de RegularizacionDeMaterias_nuevo.pas (solapa CNA), sin staging.
/// </summary>
public sealed record RegularizacionCnaDto
{
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public short Cutuco { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? Condicion { get; init; }

    /// <summary>Nota final ya cargada (CURSADA.FINAL1), si la hubiera.</summary>
    public decimal? NotaFinal { get; init; }

    /// <summary>Fecha del examen final ya cargada (CURSADA.FECHA1), si la hubiera.</summary>
    public DateTime? Fecha { get; init; }
}
