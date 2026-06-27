namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Un alumno candidato a cargar nota de final en una mesa: lo que la grilla de
/// carga muestra y edita. Réplica del SELECT de FinalesxMesayComision.BuscarMesaClick
/// (XXX_MESAS_ALUMNOS ⨝ CURSADA ⨝ ALUMNOS), con las notas/fechas/actas actuales de
/// CURSADA precargadas y NUMFIN = cuántos llamados a final ya tiene rendidos (1..4).
/// </summary>
public sealed record CargaFinalAlumnoDto
{
    public required string CodigoAlumno { get; init; }

    public required string CodigoCarrera { get; init; }

    public required string CodigoMateria { get; init; }

    public int Mesa { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    public string? SiglaMateria { get; init; }

    /// <summary>Condición actual del alumno en la materia (CURSADA.CONDICION).</summary>
    public string? Condicion { get; init; }

    /// <summary>NUMFIN: número de llamado a final que corresponde cargar (1..4).</summary>
    public int NumeroFinal { get; init; }

    public decimal? NotaFinal1 { get; init; }

    public DateOnly? FechaFinal1 { get; init; }

    public decimal? NotaFinal2 { get; init; }

    public DateOnly? FechaFinal2 { get; init; }

    public decimal? NotaFinal3 { get; init; }

    public DateOnly? FechaFinal3 { get; init; }

    public string? ActaFinal1 { get; init; }

    public string? ActaFinal2 { get; init; }

    public string? ActaFinal3 { get; init; }
}
