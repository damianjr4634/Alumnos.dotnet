namespace Esba.Domain.Entities;

/// <summary>
/// Tabla MATERIAS: catálogo de materias por carrera. PK: (CODMATERI, CODCARRE).
/// ABM legacy: altamodifmaterias.pas.
/// </summary>
public class Materia
{
    /// <summary>CODMATERI CHAR(2), parte de la PK.</summary>
    public required string Codigo { get; set; }

    /// <summary>CODCARRE VARCHAR(6), parte de la PK.</summary>
    public required string CodigoCarrera { get; set; }

    public Carrera? Carrera { get; set; }

    /// <summary>DESCRIPCI VARCHAR(60): nombre de la materia.</summary>
    public string? Nombre { get; set; }

    /// <summary>SIGLA VARCHAR(30): nombre corto para grillas.</summary>
    public string? Sigla { get; set; }

    /// <summary>CUATRIM SMALLINT: cuatrimestre del plan en que se dicta (el trigger CURSADA_BI0 lo usa para resolver el '9' de CUTUCO).</summary>
    public short? Cuatrimestre { get; set; }

    /// <summary>EQUIVALE CHAR(2): código de materia equivalente. ⚠️ semántica exacta a confirmar.</summary>
    public string? CodigoEquivalencia { get; set; }

    /// <summary>CORRELATIV VARCHAR(100): códigos de materias correlativas para cursar.</summary>
    public string? CorrelativasCursada { get; set; }

    /// <summary>CORREFINAL VARCHAR(100): códigos de correlativas para rendir el final.</summary>
    public string? CorrelativasFinal { get; set; }

    /// <summary>LAB CHAR(1) 'S'/'N': materia de laboratorio. ⚠️ semántica inferida.</summary>
    public bool? EsLaboratorio { get; set; }

    /// <summary>ESTADO CHAR(1). ⚠️ semántica a confirmar.</summary>
    public string? Estado { get; set; }

    /// <summary>ANUAL CHAR(1) 'S'/'N': materia anual (no cuatrimestral).</summary>
    public bool? EsAnual { get; set; }

    /// <summary>CODANUAL VARCHAR(2): código de la contraparte anual. ⚠️ semántica a confirmar.</summary>
    public string? CodigoAnual { get; set; }

    /// <summary>CODNEW CHAR(2): código nuevo en remapeos de plan. ⚠️ posible resto de migraciones de plan.</summary>
    public string? CodigoNuevo { get; set; }

    /// <summary>PROMOCION CHAR(1) 'S'/'N': admite promoción sin final. ⚠️ semántica inferida.</summary>
    public bool? AdmitePromocion { get; set; }

    /// <summary>APRSFINAL VARCHAR(1). ⚠️ semántica a confirmar (¿aprueba sin final?).</summary>
    public string? ApruebaSinFinal { get; set; }

    /// <summary>APTSFINAL VARCHAR(5). ⚠️ semántica a confirmar (¿nota mínima para promoción?).</summary>
    public string? AptoSinFinal { get; set; }

    /// <summary>ORDEN SMALLINT: orden en el plan.</summary>
    public short? Orden { get; set; }

    /// <summary>USUARIO VARCHAR(15): última modificación.</summary>
    public string? Usuario { get; set; }
}
