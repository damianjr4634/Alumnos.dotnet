namespace Esba.Domain.Entities;

/// <summary>
/// Tabla COMARM ("comisiones armadas"): comisión de cursada de una materia en
/// un cuatrimestre. PK: (CARRE, CUTUCO, COD_MAT, CUA_ANIO).
/// ABM legacy: cargacomisiones.pas (valida con SP XXX_VALIDO_COMISION —
/// // TODO-migrar wrapper 2.B cuando se migre el ABM).
/// </summary>
public class Comision
{
    /// <summary>CARRE VARCHAR(6), parte de la PK.</summary>
    public required string CodigoCarrera { get; set; }

    /// <summary>
    /// CUTUCO SMALLINT, parte de la PK: código compuesto CUatrimestre-TUrno-COmisión
    /// (3 dígitos). Al insertar cursada se usa 9TC y el trigger resuelve el 9 con
    /// MATERIAS.CUATRIM.
    /// </summary>
    public short Cutuco { get; set; }

    /// <summary>COD_MAT CHAR(2), parte de la PK.</summary>
    public required string CodigoMateria { get; set; }

    public Materia? Materia { get; set; }

    /// <summary>CUA_ANIO CHAR(3), parte de la PK: cuatrimestre (1) + año (2), ej. "124" = 1/24.</summary>
    public required string CuatrimestreAnio { get; set; }

    /// <summary>CODPROFES CHAR(3): código del docente (FK implícita a DOCENTES). // TODO-migrar DOCENTES</summary>
    public string? CodigoProfesor { get; set; }

    /// <summary>DIA1/BLOQUE1..DIA3/BLOQUE3: hasta tres días y bloques horarios de dictado.</summary>
    public string? Dia1 { get; set; }

    public string? Bloque1 { get; set; }

    public string? Dia2 { get; set; }

    public string? Bloque2 { get; set; }

    public string? Dia3 { get; set; }

    public string? Bloque3 { get; set; }

    /// <summary>TIT_SUP VARCHAR(2): titular/suplente. ⚠️ valores exactos a confirmar.</summary>
    public string? TitularSuplente { get; set; }

    /// <summary>USUARIO VARCHAR(15): última modificación.</summary>
    public string? Usuario { get; set; }
}
