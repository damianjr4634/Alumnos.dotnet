namespace Esba.Domain.Entities;

/// <summary>
/// Tabla TBL_FALTAS: catálogo de tipos de inasistencia. PK: FCODIGO. Un tipo
/// aplica a una carrera si <see cref="Carreras"/> es null (global) o la contiene
/// (legacy: "CARRE IS NULL OR CARRE CONTAINING vcarrera").
/// </summary>
public class TipoFalta
{
    /// <summary>FCODIGO VARCHAR(2): código del tipo (PK).</summary>
    public required string Codigo { get; set; }

    /// <summary>FDESCRI VARCHAR(30): descripción.</summary>
    public string? Descripcion { get; set; }

    /// <summary>FCANTID NUMERIC(5,2): cantidad de faltas que computa el tipo.</summary>
    public decimal Cantidad { get; set; }

    /// <summary>FJUSTIF CHAR(1) 'S'/'N': la falta está justificada.</summary>
    public bool Justifica { get; set; }

    /// <summary>FTIPO VARCHAR(2): subtipo. ⚠️ semántica exacta a confirmar.</summary>
    public string? Tipo { get; set; }

    /// <summary>CARRE VARCHAR(100): carreras a las que aplica (lista); null = todas.</summary>
    public string? Carreras { get; set; }
}
