using Esba.Domain.Enums;

namespace Esba.Domain.Entities;

/// <summary>
/// Lo que las reglas de autorización necesitan de cualquier precarga docente (comisión o
/// mesa): quién es el titular y en qué estado está. Lo implementan las dos cabeceras.
/// </summary>
public interface ICargaDocente
{
    /// <summary>CODPROFES del titular al crear la carga.</summary>
    string CodigoDocente { get; }

    EstadoCargaDocente Estado { get; }
}

/// <summary>
/// Tabla DOC_CARGA_MESA (hito 19, migración 2026-10-03): cabecera de la precarga de notas
/// de final que el docente titular hace sobre una mesa de examen y que secretaría luego
/// efectiviza con la carga de notas de finales (hito 14). Una por mesa (CARRE, MESA); el
/// estado vive acá. Sin FK a MESAS/PERMEXA (el escritorio las reescribe).
/// PK: ID (trigger DOC_CARGA_MESA_BI0 con GEN_ID(G_DOC_CARGA_MESA)).
/// </summary>
public class CargaMesaDocente : ICargaDocente
{
    public int Id { get; set; }

    /// <summary>CARRE VARCHAR(6).</summary>
    public required string CodigoCarrera { get; set; }

    /// <summary>MESA INTEGER (MESAS.MESA).</summary>
    public int NumeroMesa { get; set; }

    /// <summary>CODPROFES CHAR(3): titular de la mesa al crear la carga (MESAS.TITULAR).</summary>
    public required string CodigoDocente { get; set; }

    /// <summary>ESTADO CHAR(3) 'BOR'/'FIN'/'EFE' (CHECK en la base).</summary>
    public EstadoCargaDocente Estado { get; set; } = EstadoCargaDocente.Borrador;

    /// <summary>OBSERV VARCHAR(500).</summary>
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

    public ICollection<CargaMesaDocenteDetalle> Detalles { get; set; } = [];

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
/// Tabla DOC_CARGA_MESA_DET: un alumno con permiso en la mesa, con su nota de final
/// precargada o la marca de ausente. PERMEXA_INDICE es ayuda, no FK.
/// PK: ID (trigger DOC_CARGA_MESA_DET_BI0). Único por (CARGA_ID, COD_ALU, COD_MAT).
/// </summary>
public class CargaMesaDocenteDetalle
{
    public int Id { get; set; }

    public int CargaId { get; set; }

    public CargaMesaDocente? Carga { get; set; }

    /// <summary>COD_ALU CHAR(11).</summary>
    public required string CodigoAlumno { get; set; }

    /// <summary>COD_MAT CHAR(2): materia del permiso (la de la mesa).</summary>
    public required string CodigoMateria { get; set; }

    /// <summary>PERMEXA.INDICE del permiso (ayuda para efectivizar).</summary>
    public int? PermisoIndice { get; set; }

    /// <summary>NOTA NUMERIC(5,2): nota de final; null si no rindió o está ausente.</summary>
    public decimal? Nota { get; set; }

    /// <summary>AUSENTE CHAR(1) 'S'/'N': no se presentó (distinto de nota sin cargar).</summary>
    public bool Ausente { get; set; }

    /// <summary>OBSERV VARCHAR(500).</summary>
    public string? Observaciones { get; set; }

    public DateTime FechaModificacion { get; set; }

    /// <summary>CODUSU_MODIF: quién tocó la fila por última vez (docente o secretaría).</summary>
    public int CodigoUsuarioModificacion { get; set; }

    /// <summary>true si no hay nada cargado (ni nota, ni ausente, ni observación).</summary>
    public bool EstaVacio => Nota is null && !Ausente && string.IsNullOrWhiteSpace(Observaciones);
}
