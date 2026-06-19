namespace Esba.Domain.Entities;

/// <summary>
/// Tabla ANALITIC: histórico académico / analítico del alumno (finales rendidos,
/// materias aprobadas por equivalencia o pase). PK: (CARRE, COD_ALU, COD_MAT).
/// Distinta de <see cref="Cursada"/> (el cursado en curso): acá van las materias ya
/// resueltas que componen el analítico.
/// Lógica en triggers (se conservan): INDICE por generador G_ANALITIC, ULTMOD,
/// padding de ACTINT/ACTDGE a 15 con ceros, y la regla de que una materia NO puede
/// estar en CURSADA y ANALITIC a la vez (BI0 lanza excepción). La auditoría
/// (LOG_ANALITIC) la escribe un trigger AFTER: desde .NET no se toca.
/// </summary>
public class Analitico
{
    /// <summary>CARRE VARCHAR(6), parte de la PK.</summary>
    public required string CodigoCarrera { get; set; }

    /// <summary>COD_ALU CHAR(11), parte de la PK.</summary>
    public required string CodigoAlumno { get; set; }

    /// <summary>COD_MAT CHAR(2), parte de la PK.</summary>
    public required string CodigoMateria { get; set; }

    public Materia? Materia { get; set; }

    /// <summary>APELLIDO CHAR(25): denormalizado desde ALUMNOS.</summary>
    public string? Apellido { get; set; }

    /// <summary>CUA_ANIO CHAR(3): cuatrimestre (1) + año (2). El trigger nullea el vacío.</summary>
    public string? CuatrimestreAnio { get; set; }

    /// <summary>NOTA_MAT NUMERIC(5,2): nota del final/equivalencia.</summary>
    public decimal? Nota { get; set; }

    /// <summary>FEC_FINAL DATE: fecha del final (el trigger nullea el sentinela 1899-12-30).</summary>
    public DateOnly? FechaFinal { get; set; }

    /// <summary>
    /// CONDICION CHAR(15): 'EQUIVALENCIA', 'REGULAR', 'PASE/FINAL', etc. ⚠️ Texto libre,
    /// no se modela como enum hasta sanear el dominio (igual que <see cref="Cursada"/>).
    /// </summary>
    public string? Condicion { get; set; }

    /// <summary>MATRIZ CHAR(5): libro matriz; el trigger no permite blanquearlo en UPDATE.</summary>
    public string? Matriz { get; set; }

    // --- Origen de la equivalencia / pase ---

    /// <summary>INSTITUT CHAR(30): institución de origen.</summary>
    public string? Instituto { get; set; }

    /// <summary>CARAC CHAR(6): característica de la institución de origen.</summary>
    public string? Caracteristica { get; set; }

    /// <summary>ACTINT VARCHAR(15): N° de acta interna (el trigger lo rellena con LPAD a 15 ceros).</summary>
    public string? ActaInterna { get; set; }

    /// <summary>ACTDGE VARCHAR(15): N° de acta D.G.E.G.P. (el trigger lo rellena con LPAD a 15 ceros).</summary>
    public string? ActaDge { get; set; }

    /// <summary>ACTSNE VARCHAR(10): N° de acta S.N.E.</summary>
    public string? ActaSne { get; set; }

    /// <summary>COLEGIO CHAR(40): colegio de origen.</summary>
    public string? Colegio { get; set; }

    /// <summary>PLAN CHAR(40): plan de origen (columna con comillas en Firebird).</summary>
    public string? Plan { get; set; }

    /// <summary>A_C CHAR(1): 'C' constancia / 'A' analítico.</summary>
    public string? Ac { get; set; }

    /// <summary>NREG NUMERIC(5,0): número de registro.</summary>
    public int? NumeroRegistro { get; set; }

    /// <summary>FEQDOCE VARCHAR(3): docente de la materia equivalida (origen).</summary>
    public string? EquivDocente { get; set; }

    /// <summary>FEQMATE VARCHAR(50): nombre de la materia cursada en origen.</summary>
    public string? EquivMateria { get; set; }

    /// <summary>FEQCARRE VARCHAR(100): carrera cursada en origen.</summary>
    public string? EquivCarrera { get; set; }

    /// <summary>FEQINST VARCHAR(100): institución cursada en origen.</summary>
    public string? EquivInstituto { get; set; }

    /// <summary>FACTFIN VARCHAR(10): folio/acta del final.</summary>
    public string? ActaFinal { get; set; }

    /// <summary>FEXDESCRI VARCHAR(200): descripción de la eximición (EXIMDESC en las constancias).</summary>
    public string? EximidoDescripcion { get; set; }

    /// <summary>INDICE INTEGER NOT NULL: surrogate por trigger (GEN_ID(G_ANALITIC)).</summary>
    public int Indice { get; set; }

    /// <summary>USUARIO VARCHAR(15): última modificación.</summary>
    public string? Usuario { get; set; }

    /// <summary>ULTMOD TIMESTAMP: mantenida por el trigger.</summary>
    public DateTime? UltimaModificacion { get; set; }
}
