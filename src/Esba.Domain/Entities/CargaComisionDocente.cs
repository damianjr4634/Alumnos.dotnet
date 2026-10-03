using Esba.Domain.Enums;

namespace Esba.Domain.Entities;

/// <summary>
/// Tabla DOC_CARGA_COMISION (hito 19, migración 2026-10-03): cabecera de la precarga
/// que el docente titular hace sobre una comisión (notas del cursado + totales de
/// horas e inasistencias) y que secretaría luego efectiviza sobre CURSADA. Una por
/// comisión (CARRE, CUTUCO, COD_MAT, CUA_ANIO); el estado vive acá, no por alumno.
/// Sin FK a COMARM (el escritorio la reescribe); los detalles sí cuelgan por FK.
/// PK: ID (trigger DOC_CARGA_COMISION_BI0 con GEN_ID(G_DOC_CARGA_COMISION)).
/// </summary>
public class CargaComisionDocente : ICargaDocente
{
    public int Id { get; set; }

    /// <summary>CARRE VARCHAR(6).</summary>
    public required string CodigoCarrera { get; set; }

    /// <summary>CUTUCO SMALLINT.</summary>
    public short Cutuco { get; set; }

    /// <summary>COD_MAT CHAR(2).</summary>
    public required string CodigoMateria { get; set; }

    /// <summary>CUA_ANIO CHAR(3).</summary>
    public required string CuatrimestreAnio { get; set; }

    /// <summary>CODPROFES CHAR(3): docente titular de la comisión al crear la carga (COMARM.CODPROFES).</summary>
    public required string CodigoDocente { get; set; }

    /// <summary>ESTADO CHAR(3) 'BOR'/'FIN'/'EFE' (CHECK en la base).</summary>
    public EstadoCargaDocente Estado { get; set; } = EstadoCargaDocente.Borrador;

    /// <summary>OBSERV VARCHAR(500): observaciones generales del docente.</summary>
    public string? Observaciones { get; set; }

    public DateTime FechaAlta { get; set; }

    public int CodigoUsuarioAlta { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public int? CodigoUsuarioModificacion { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public int? CodigoUsuarioFinalizacion { get; set; }

    public DateTime? FechaReapertura { get; set; }

    public int? CodigoUsuarioReapertura { get; set; }

    public DateTime? FechaEfectivizacion { get; set; }

    public int? CodigoUsuarioEfectivizacion { get; set; }

    public ICollection<CargaComisionDocenteDetalle> Detalles { get; set; } = [];

    /// <summary>El docente solo edita mientras la carga está en borrador.</summary>
    public bool EditablePorDocente => Estado == EstadoCargaDocente.Borrador;

    public void Finalizar(int codigoUsuario, DateTime ahora)
    {
        Estado = EstadoCargaDocente.Finalizada;
        FechaFinalizacion = ahora;
        CodigoUsuarioFinalizacion = codigoUsuario;
    }

    public void Reabrir(int codigoUsuario, DateTime ahora)
    {
        Estado = EstadoCargaDocente.Borrador;
        FechaReapertura = ahora;
        CodigoUsuarioReapertura = codigoUsuario;
    }

    public void Efectivizar(int codigoUsuario, DateTime ahora)
    {
        Estado = EstadoCargaDocente.Efectivizada;
        FechaEfectivizacion = ahora;
        CodigoUsuarioEfectivizacion = codigoUsuario;
    }

    public void RegistrarModificacion(int codigoUsuario, DateTime ahora)
    {
        FechaModificacion = ahora;
        CodigoUsuarioModificacion = codigoUsuario;
    }
}

/// <summary>
/// Tabla DOC_CARGA_COMISION_DET: un alumno de la comisión con sus notas precargadas.
/// Mismos campos que CURSADA (TP_EVA, RECUP, TP_EVA2, RECUP2, TP_EVA3, TOT_HORAS,
/// INASIST, JUSTIF) para que efectivizar sea copiar 1:1. CURSADA_INDICE es ayuda, no FK.
/// PK: ID (trigger DOC_CARGA_COMISION_DET_BI0). Único por (CARGA_ID, COD_ALU).
/// </summary>
public class CargaComisionDocenteDetalle
{
    public int Id { get; set; }

    public int CargaId { get; set; }

    public CargaComisionDocente? Carga { get; set; }

    /// <summary>COD_ALU CHAR(11).</summary>
    public required string CodigoAlumno { get; set; }

    /// <summary>CURSADA.INDICE de la fila que se va a efectivizar (ayuda; se re-resuelve por clave al efectivizar).</summary>
    public int? CursadaIndice { get; set; }

    public decimal? Evaluacion1 { get; set; }

    /// <summary>RECUP: el único recuperatorio que usan las variantes terciaria y bachillerato.</summary>
    public decimal? Recuperatorio1 { get; set; }

    public decimal? Evaluacion2 { get; set; }

    /// <summary>RECUP2: ninguna variante de regularización lo edita; la columna se conserva pero no se expone.</summary>
    public decimal? Recuperatorio2 { get; set; }

    /// <summary>TP_EVA3: 3° trimestre del secundario (333/650).</summary>
    public decimal? Evaluacion3 { get; set; }

    /// <summary>REGULAR (migración 2026-10-04): nota "a regularizar" del bachillerato.</summary>
    public decimal? NotaRegular { get; set; }

    /// <summary>FINAL1 (migración 2026-10-04): nota final de CNA.</summary>
    public decimal? NotaFinal { get; set; }

    public short? TotalHoras { get; set; }

    public short? Inasistencias { get; set; }

    public short? Justificadas { get; set; }

    /// <summary>OBSERV VARCHAR(500): nota libre del docente sobre el alumno.</summary>
    public string? Observaciones { get; set; }

    public DateTime FechaModificacion { get; set; }

    /// <summary>CODUSU_MODIF: quién tocó la fila por última vez (docente o secretaría, ambos son USUARIOS).</summary>
    public int CodigoUsuarioModificacion { get; set; }

    /// <summary>true si no hay ningún valor cargado (fila vacía).</summary>
    public bool EstaVacio =>
        Evaluacion1 is null && Recuperatorio1 is null && Evaluacion2 is null && Recuperatorio2 is null
        && Evaluacion3 is null && NotaRegular is null && NotaFinal is null
        && TotalHoras is null && Inasistencias is null && Justificadas is null
        && string.IsNullOrWhiteSpace(Observaciones);
}
