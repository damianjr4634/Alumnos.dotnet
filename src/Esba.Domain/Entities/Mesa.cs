namespace Esba.Domain.Entities;

/// <summary>
/// Tabla MESAS: mesa de examen de una materia en una carrera. PK: (CARRE, MESA).
/// ABM legacy: MesasExamen.pas (valida el alta con XXX_VALIDO_MESA).
/// </summary>
public class Mesa
{
    /// <summary>CARRE VARCHAR(6), parte de la PK.</summary>
    public required string CodigoCarrera { get; set; }

    /// <summary>MESA INTEGER, parte de la PK: número de mesa.</summary>
    public int NumeroMesa { get; set; }

    /// <summary>COD_MAT CHAR(2): materia.</summary>
    public string? CodigoMateria { get; set; }

    /// <summary>LLAMADO NUMERIC(1): número de llamado.</summary>
    public short? Llamado { get; set; }

    /// <summary>FECH_EXA DATE: fecha del examen.</summary>
    public DateOnly? FechaExamen { get; set; }

    /// <summary>HORA NUMERIC(4): hora (entero, ej. 1830).</summary>
    public short? Hora { get; set; }

    /// <summary>TITULAR CHAR(3): docente titular.</summary>
    public string? Titular { get; set; }

    /// <summary>VOCAL1 CHAR(3): primer vocal.</summary>
    public string? Vocal1 { get; set; }

    /// <summary>VOCAL2 CHAR(3): segundo vocal.</summary>
    public string? Vocal2 { get; set; }

    /// <summary>AULA NUMERIC(2): aula.</summary>
    public short? Aula { get; set; }

    /// <summary>CUATRIM NUMERIC(1): cuatrimestre. No se carga desde el formulario legacy.</summary>
    public short? Cuatrimestre { get; set; }

    /// <summary>COMI1/COMI2/COMI3 NUMERIC(3): comisiones que rinden en la mesa.</summary>
    public short? Comision1 { get; set; }

    public short? Comision2 { get; set; }

    public short? Comision3 { get; set; }

    /// <summary>TIPMES CHAR(2): tipo de mesa (FK a MESA_TIPO).</summary>
    public string? CodigoTipo { get; set; }

    /// <summary>USUARIO CHAR(15): última modificación (el legacy guarda el CodUsu).</summary>
    public string? Usuario { get; set; }

    /// <summary>NREG NUMERIC(10): número de registro (lo asigna la base). ⚠️ a confirmar.</summary>
    public long? NumeroRegistro { get; set; }

    /// <summary>ULTMOD TIMESTAMP: última modificación.</summary>
    public DateTime? UltimaModificacion { get; set; }
}
