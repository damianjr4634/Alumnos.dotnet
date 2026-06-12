namespace Esba.Domain.Entities;

/// <summary>
/// Tabla CURSADA: la cursada de una materia por un alumno (notas, asistencia,
/// finales, condición). PK: (CARRE, COD_ALU, COD_MAT).
/// Mucha lógica vive en el trigger CURSADA_BI0 (se conserva): INDICE por
/// generador, denormalización de APELLIDO/MATRIZ desde ALUMNOS, resolución del
/// '9' de CUTUCO con MATERIAS.CUATRIM, sincronización con RECURSA según
/// CONDICION, y reseteo de finales desaprobados al pasar a RECURSA.
/// La auditoría (LOG_CURSADA) la escribe un trigger AFTER: desde .NET no se toca.
/// </summary>
public class Cursada
{
    /// <summary>CARRE VARCHAR(6), parte de la PK.</summary>
    public required string CodigoCarrera { get; set; }

    /// <summary>COD_ALU CHAR(11), parte de la PK.</summary>
    public required string CodigoAlumno { get; set; }

    /// <summary>COD_MAT CHAR(2), parte de la PK.</summary>
    public required string CodigoMateria { get; set; }

    public Materia? Materia { get; set; }

    /// <summary>APELLIDO CHAR(25): denormalizado desde ALUMNOS por el trigger si viene vacío.</summary>
    public string? Apellido { get; set; }

    /// <summary>CUTUCO NUMERIC(3,0): cuatrimestre+turno+comisión (ver Comision.Cutuco).</summary>
    public short? Cutuco { get; set; }

    /// <summary>CUA_ANIO CHAR(3): cuatrimestre (1) + año (2). El trigger exige no vacío.</summary>
    public string? CuatrimestreAnio { get; set; }

    /// <summary>
    /// CONDICION CHAR(15) NOT NULL. ⚠️ Texto libre con variantes históricas en los
    /// datos (REGULAR, LIBRE, RECURSA, PREVIA/PREVIO, P/EQUIVALEN, RECURSANDO,
    /// A/REGULAR, ENPROCESO, MARZO, DICIEMBRE, REINCORPORA, PASE/REGULAR, 0LIBRES):
    /// NO se modela como enum hasta sanear el dominio. El trigger sincroniza
    /// RECURSA cuando vale RECURSANDO.
    /// </summary>
    public required string Condicion { get; set; }

    // --- Evaluaciones del cursado (NUMERIC(5,2)) ---

    /// <summary>TP_EVA: primera evaluación/TP.</summary>
    public decimal? Evaluacion1 { get; set; }

    /// <summary>RECUP: recuperatorio de la primera evaluación.</summary>
    public decimal? Recuperatorio1 { get; set; }

    /// <summary>TP_EVA2.</summary>
    public decimal? Evaluacion2 { get; set; }

    /// <summary>RECUP2.</summary>
    public decimal? Recuperatorio2 { get; set; }

    /// <summary>TP_EVA3.</summary>
    public decimal? Evaluacion3 { get; set; }

    /// <summary>REGULAR: nota de regularización.</summary>
    public decimal? NotaRegular { get; set; }

    /// <summary>PROM: promedio del cursado.</summary>
    public decimal? Promedio { get; set; }

    /// <summary>FEC_EVA1..3: fechas de las evaluaciones.</summary>
    public DateOnly? FechaEvaluacion1 { get; set; }

    public DateOnly? FechaEvaluacion2 { get; set; }

    public DateOnly? FechaEvaluacion3 { get; set; }

    /// <summary>FAL_EVA1..3 NUMERIC(9,2): faltas al momento de cada evaluación.</summary>
    public decimal? FaltasEvaluacion1 { get; set; }

    public decimal? FaltasEvaluacion2 { get; set; }

    public decimal? FaltasEvaluacion3 { get; set; }

    // --- Asistencia ---

    /// <summary>TOT_HORAS NUMERIC(3,0).</summary>
    public short? TotalHoras { get; set; }

    /// <summary>INASIST NUMERIC(3,0).</summary>
    public short? Inasistencias { get; set; }

    /// <summary>JUSTIF NUMERIC(3,0): inasistencias justificadas.</summary>
    public short? Justificadas { get; set; }

    // --- Finales (hasta 4 llamados) ---

    /// <summary>FINAL1..4 + FECHA1..4: notas y fechas de los llamados a final.</summary>
    public decimal? NotaFinal1 { get; set; }

    public DateOnly? FechaFinal1 { get; set; }

    public decimal? NotaFinal2 { get; set; }

    public DateOnly? FechaFinal2 { get; set; }

    public decimal? NotaFinal3 { get; set; }

    public DateOnly? FechaFinal3 { get; set; }

    public decimal? NotaFinal4 { get; set; }

    public DateOnly? FechaFinal4 { get; set; }

    /// <summary>FACTFIN1..3 VARCHAR(10). ⚠️ ¿folio/acta del final? — a confirmar.</summary>
    public string? ActaFinal1 { get; set; }

    public string? ActaFinal2 { get; set; }

    public string? ActaFinal3 { get; set; }

    /// <summary>NOTADIC/FECHDIC y NOTAMAR/FECHMAR: llamados de diciembre y marzo.</summary>
    public decimal? NotaDiciembre { get; set; }

    public DateOnly? FechaDiciembre { get; set; }

    public decimal? NotaMarzo { get; set; }

    public DateOnly? FechaMarzo { get; set; }

    // --- Origen / equivalencias ---

    /// <summary>MATRIZ CHAR(5): denormalizado desde ALUMNOS por el trigger; no se blanquea.</summary>
    public string? Matriz { get; set; }

    /// <summary>INSTITUT CHAR(30) / CARAC CHAR(5): institución de origen para equivalencias.</summary>
    public string? Instituto { get; set; }

    public string? Caracteristica { get; set; }

    /// <summary>ACTINT/ACTDGE/ACTSNE: números de acta (interna, DGE, SNE). ⚠️ semántica a confirmar.</summary>
    public string? ActaInterna { get; set; }

    public string? ActaDge { get; set; }

    public string? ActaSne { get; set; }

    /// <summary>NREG NUMERIC(5,0): número de registro.</summary>
    public int? NumeroRegistro { get; set; }

    /// <summary>COLEGIO CHAR(40): colegio de origen (equivalencias).</summary>
    public string? Colegio { get; set; }

    /// <summary>PLAN CHAR(15) (columna con comillas en Firebird).</summary>
    public string? Plan { get; set; }

    /// <summary>A_C CHAR(1). ⚠️ semántica a confirmar.</summary>
    public string? Ac { get; set; }

    /// <summary>DEFINE CHAR(1). ⚠️ semántica a confirmar.</summary>
    public string? Define { get; set; }

    /// <summary>INDICE INTEGER NOT NULL: surrogate por trigger (GEN_ID(G_CURSADA)).</summary>
    public int Indice { get; set; }

    /// <summary>USUARIO VARCHAR(15): última modificación.</summary>
    public string? Usuario { get; set; }

    /// <summary>ULTMOD TIMESTAMP: mantenida por el trigger.</summary>
    public DateTime? UltimaModificacion { get; set; }
}
