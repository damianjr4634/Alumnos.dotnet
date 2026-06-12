using Esba.Domain.Enums;

namespace Esba.Domain.Entities;

/// <summary>
/// Tabla ALUMNOS: padrón de alumnos por carrera. PK compuesta: (CARRE, COD_ALU).
/// La base legacy no declara FK hacia CARRERA: la relación es implícita y se
/// modela solo en EF Core.
/// Columnas excluidas a propósito (decisiones 2026-06-12):
/// - BAJAADM/ADMEST/ADMCURSO: módulo administrativo fuera de alcance (las completan los triggers).
/// - MATRIZ_L/MATRIZ_F: calculadas (SUBSTRING de MATRIZ).
/// - UNIVER/AUNIVER/TUNIVER/OUNIVER, ESPEC/AESPEC/TESPEC/OESPEC, MOROSO/MORO1..3/F_MORO,
///   CD, RESIDE, FFECLIB: columnas muertas confirmadas por el usuario — no se migran.
/// </summary>
public class Alumno
{
    /// <summary>COD_ALU CHAR(11), parte de la PK: tipo de documento (3) + número (8). Ver ValueObjects.CodigoAlumno.</summary>
    public required string Codigo { get; set; }

    /// <summary>CARRE VARCHAR(6), parte de la PK.</summary>
    public required string CodigoCarrera { get; set; }

    public Carrera? Carrera { get; set; }

    /// <summary>MATRIZ CHAR(5): libro matriz (libro 2 + folio 3). No puede blanquearse una vez asignado (trigger ALUMNOS_BU0). Ver ValueObjects.LibroMatriz.</summary>
    public string? Matriz { get; set; }

    /// <summary>APELLIDO CHAR(25).</summary>
    public string? Apellido { get; set; }

    /// <summary>NOM_APE CHAR(25): nombres.</summary>
    public string? Nombre { get; set; }

    /// <summary>EXT_POR CHAR(10): expedido por (organismo que expidió el documento).</summary>
    public string? DocumentoExpedidoPor { get; set; }

    public Sexo? Sexo { get; set; }

    /// <summary>NACIONAL CHAR(15).</summary>
    public string? Nacionalidad { get; set; }

    public EstadoCivil? EstadoCivil { get; set; }

    /// <summary>FEC_NAC DATE.</summary>
    public DateOnly? FechaNacimiento { get; set; }

    /// <summary>LUG_NAC CHAR(15).</summary>
    public string? LugarNacimiento { get; set; }

    /// <summary>PCIA_NAC VARCHAR(20).</summary>
    public string? ProvinciaNacimiento { get; set; }

    /// <summary>DOMI CHAR(30).</summary>
    public string? Domicilio { get; set; }

    /// <summary>LOCALI VARCHAR(40).</summary>
    public string? Localidad { get; set; }

    /// <summary>COD_POS NUMERIC(4,0).</summary>
    public short? CodigoPostal { get; set; }

    /// <summary>CAR_TEL VARCHAR(7): característica telefónica.</summary>
    public string? CaracteristicaTelefono { get; set; }

    /// <summary>TELE VARCHAR(15).</summary>
    public string? Telefono { get; set; }

    /// <summary>FEC_ING DATE: fecha de ingreso al instituto.</summary>
    public DateOnly? FechaIngreso { get; set; }

    // --- Estudios previos (grupos CPRIM/CSECU/TERCI/UNIVER/ESPEC del formulario de alta) ---

    /// <summary>CPRIM VARCHAR(50): colegio primario.</summary>
    public string? ColegioPrimario { get; set; }

    /// <summary>APRIM CHAR(2): año de egreso del primario.</summary>
    public string? AnioPrimario { get; set; }

    /// <summary>TPRIM VARCHAR(50): título del primario.</summary>
    public string? TituloPrimario { get; set; }

    /// <summary>OPRIM CHAR(10). ⚠️ no se usa en FrmAltaAlumno (posible columna muerta).</summary>
    public string? ObservacionPrimario { get; set; }

    /// <summary>CSECU VARCHAR(50): colegio secundario.</summary>
    public string? ColegioSecundario { get; set; }

    /// <summary>ASECU CHAR(2): año de egreso del secundario.</summary>
    public string? AnioSecundario { get; set; }

    /// <summary>TSECU VARCHAR(50): título del secundario.</summary>
    public string? TituloSecundario { get; set; }

    /// <summary>OSECU CHAR(10). ⚠️ no se usa en FrmAltaAlumno (posible columna muerta).</summary>
    public string? ObservacionSecundario { get; set; }

    /// <summary>TERCI VARCHAR(50): institución terciaria.</summary>
    public string? InstitucionTerciaria { get; set; }

    /// <summary>ATERCI CHAR(2): año de egreso del terciario.</summary>
    public string? AnioTerciario { get; set; }

    /// <summary>TTERCI VARCHAR(50): título terciario.</summary>
    public string? TituloTerciario { get; set; }

    /// <summary>OTERCI CHAR(10). ⚠️ no se usa en FrmAltaAlumno (posible columna muerta).</summary>
    public string? ObservacionTerciario { get; set; }

    // --- Datos laborales ---

    /// <summary>EMPRE CHAR(20).</summary>
    public string? Empresa { get; set; }

    /// <summary>RUBRO CHAR(20).</summary>
    public string? Rubro { get; set; }

    /// <summary>CARGO CHAR(20).</summary>
    public string? Cargo { get; set; }

    /// <summary>ANTI CHAR(10): antigüedad laboral.</summary>
    public string? Antiguedad { get; set; }

    /// <summary>DOMI_1 CHAR(25): domicilio laboral.</summary>
    public string? DomicilioLaboral { get; set; }

    /// <summary>TELE_1 CHAR(8): teléfono laboral.</summary>
    public string? TelefonoLaboral { get; set; }

    /// <summary>INTER CHAR(5): interno telefónico.</summary>
    public string? InternoLaboral { get; set; }

    // --- Documentación presentada ---

    /// <summary>PN CHAR(1): checkbox "Pn" del alta. ⚠️ persistencia no detectada en el SQL del alta.</summary>
    public string? Pn { get; set; }

    /// <summary>CTT CHAR(1): '*' si tiene certificado de título en trámite.</summary>
    public bool TieneCertificadoEnTramite { get; set; }

    /// <summary>FECH_CTT DATE: fecha del certificado en trámite.</summary>
    public DateOnly? FechaCertificadoEnTramite { get; set; }

    /// <summary>DNI CHAR(1): situación de actualización del documento (dígito '0'..'3').</summary>
    public SituacionDni? SituacionDni { get; set; }

    /// <summary>CA CHAR(1): '*' si presentó certificado de aptitud (checkbox CA del alta).</summary>
    public bool PresentoCa { get; set; }

    /// <summary>CR CHAR(1): usado solo en la exportación a Ministerio. ⚠️ semántica a confirmar.</summary>
    public string? Cr { get; set; }

    /// <summary>BAJA CHAR(1) NOT NULL: 'S'/'N'.</summary>
    public bool Baja { get; set; }

    /// <summary>INDICE INTEGER, único: surrogate generado por trigger (GEN_ID(G_ALUMNOS)). Lo usa XXX_OBSERV_PANTA.</summary>
    public int? Indice { get; set; }

    /// <summary>MAIL VARCHAR(80).</summary>
    public string? Mail { get; set; }

    /// <summary>OBSERV VARCHAR(32762): observaciones libres de la ficha.</summary>
    public string? Observaciones { get; set; }

    /// <summary>USUARIO VARCHAR(15): usuario que dio el alta/última modificación.</summary>
    public string? Usuario { get; set; }

    /// <summary>CELULAR VARCHAR(20).</summary>
    public string? Celular { get; set; }

    /// <summary>FFOTO CHAR(1): 'S'/'N' presentó foto 4x4.</summary>
    public bool? PresentoFoto { get; set; }

    /// <summary>FAPTFIS CHAR(1): 'S'/'N' presentó apto físico.</summary>
    public bool? PresentoAptoFisico { get; set; }

    /// <summary>FAPTFEC DATE: fecha del apto físico.</summary>
    public DateOnly? FechaAptoFisico { get; set; }

    /// <summary>FUSUWEB VARCHAR(20): usuario del portal web del alumno.</summary>
    public string? UsuarioWeb { get; set; }

    /// <summary>FOTO BLOB: foto del alumno. No puede borrarse una vez cargada (trigger ALUMNOS_BIU0).</summary>
    public byte[]? Foto { get; set; }

    /// <summary>FPARNAC CHAR(1): 'S'/'N' presentó partida de nacimiento.</summary>
    public bool? PresentoPartidaNacimiento { get; set; }

    /// <summary>ULTMOD TIMESTAMP: mantenida por triggers (CURRENT_TIMESTAMP en insert/update).</summary>
    public DateTime? UltimaModificacion { get; set; }

    /// <summary>FLIBRETA CHAR(1): 'S'/'N' presentó libreta.</summary>
    public bool? PresentoLibreta { get; set; }

    /// <summary>FLIBFEC DATE: fecha de la libreta.</summary>
    public DateOnly? FechaLibreta { get; set; }

    public EstadoAlumno? Estado { get; set; }

    public Genero? Genero { get; set; }

    /// <summary>NOMIPASE CHAR(1): 'S'/'N' — nómina pase (checkbox del alta).</summary>
    public bool? NominaPase { get; set; }
}
