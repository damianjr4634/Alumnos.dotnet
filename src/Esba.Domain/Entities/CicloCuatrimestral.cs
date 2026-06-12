namespace Esba.Domain.Entities;

/// <summary>
/// Tabla TBL_CUAT: fechas de los dos cuatrimestres de cada año lectivo.
/// PK: FANIO. ABM legacy: CargadeTrimestres.pas.
/// </summary>
public class CicloCuatrimestral
{
    /// <summary>FANIO INTEGER, PK: año lectivo.</summary>
    public int Anio { get; set; }

    /// <summary>FDDEPRI / FHTAPRI: desde/hasta del primer cuatrimestre.</summary>
    public DateOnly PrimerCuatrimestreDesde { get; set; }

    public DateOnly PrimerCuatrimestreHasta { get; set; }

    /// <summary>FDDESEG / FHTASEG: desde/hasta del segundo cuatrimestre.</summary>
    public DateOnly SegundoCuatrimestreDesde { get; set; }

    public DateOnly SegundoCuatrimestreHasta { get; set; }
}
