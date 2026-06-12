namespace Esba.Domain.Entities;

/// <summary>
/// Tabla TBL_TRIM: fechas de los tres trimestres de cada año lectivo
/// (modalidad trimestral). PK: FANIO. ABM legacy: CargadeTrimestres.pas.
/// </summary>
public class CicloTrimestral
{
    /// <summary>FANIO INTEGER, PK: año lectivo.</summary>
    public int Anio { get; set; }

    public DateOnly PrimerTrimestreDesde { get; set; }

    public DateOnly PrimerTrimestreHasta { get; set; }

    public DateOnly SegundoTrimestreDesde { get; set; }

    public DateOnly SegundoTrimestreHasta { get; set; }

    public DateOnly TercerTrimestreDesde { get; set; }

    public DateOnly TercerTrimestreHasta { get; set; }
}
