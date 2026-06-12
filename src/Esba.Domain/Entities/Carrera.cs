namespace Esba.Domain.Entities;

/// <summary>
/// Tabla CARRERA: oferta académica del instituto. PK: CARRE.
/// Los datos de autoridades (rector, secretaria, dirección de estudios) se
/// usan en constancias y certificados.
/// </summary>
public class Carrera
{
    /// <summary>CARRE VARCHAR(6), PK.</summary>
    public required string Codigo { get; set; }

    /// <summary>DESCARRE VARCHAR(150): nombre completo de la carrera.</summary>
    public string? Nombre { get; set; }

    /// <summary>TIPO VARCHAR(3): tipo de carrera (bachiller, bachiller a distancia, terciaria).</summary>
    public string? Tipo { get; set; }

    /// <summary>CUATRIM SMALLINT: cantidad de cuatrimestres del plan.</summary>
    public short? Cuatrimestres { get; set; }

    /// <summary>RESOLUCION VARCHAR(20): resolución ministerial.</summary>
    public string? Resolucion { get; set; }

    /// <summary>CAMINO CHAR(30). ⚠️ semántica a confirmar (parece path legacy de plantillas).</summary>
    public string? Camino { get; set; }

    /// <summary>DESCARRE2 CHAR(40): nombre alternativo usado en impresiones.</summary>
    public string? NombreAlternativo { get; set; }

    /// <summary>CUATRIM2 CHAR(1). ⚠️ semántica a confirmar.</summary>
    public string? Cuatrim2 { get; set; }

    /// <summary>RESOLU2 CHAR(15): segunda resolución (impresiones).</summary>
    public string? Resolucion2 { get; set; }

    /// <summary>RECTOR VARCHAR(50): nombre del rector para constancias.</summary>
    public string? Rector { get; set; }

    /// <summary>DNIRECTOR CHAR(10).</summary>
    public string? DniRector { get; set; }

    /// <summary>SECRETARIA CHAR(30): nombre de la secretaria para constancias.</summary>
    public string? Secretaria { get; set; }

    /// <summary>DNISECRET CHAR(10).</summary>
    public string? DniSecretaria { get; set; }

    /// <summary>USUARIO SMALLINT. ⚠️ semántica a confirmar (último usuario que modificó).</summary>
    public short? Usuario { get; set; }

    /// <summary>INSTITUT VARCHAR(30): nombre del instituto emisor.</summary>
    public string? Instituto { get; set; }

    /// <summary>CARACT VARCHAR(6): característica/código del instituto (A-781).</summary>
    public string? Caracteristica { get; set; }

    /// <summary>DIRESTU VARCHAR(30) NOT NULL: director/a de estudios.</summary>
    public required string DirectorEstudios { get; set; }

    /// <summary>DNIDIRESTU VARCHAR(15).</summary>
    public string? DniDirectorEstudios { get; set; }

    /// <summary>GRUPO CHAR(3) NOT NULL: grupo del menú por carrera (FK implícita a CARRE_GRP). // TODO-migrar CARRE_GRP</summary>
    public required string Grupo { get; set; }

    /// <summary>ORDEN SMALLINT: orden dentro del grupo del menú.</summary>
    public short? Orden { get; set; }

    /// <summary>IMAGEN INTEGER: índice de ícono del menú legacy.</summary>
    public int? Imagen { get; set; }

    /// <summary>DESCORT VARCHAR(30): nombre corto para el menú.</summary>
    public string? NombreCorto { get; set; }

    /// <summary>DISTANCIA CHAR(1) NOT NULL: 'S' modalidad a distancia, 'N' presencial (FrmEsba arma "MODALIDAD").</summary>
    public bool EsADistancia { get; set; }

    /// <summary>DESACT CHAR(1) NOT NULL: 'S' carrera en desuso (FrmEsba la excluye del padrón salvo tilde). Trigger CARRERA_BI0 la inicializa en 'N'.</summary>
    public bool Desactivada { get; set; }

    /// <summary>ORDENINF SMALLINT NOT NULL: orden en informes.</summary>
    public short OrdenInforme { get; set; }

    /// <summary>DICTAMEN VARCHAR(30): dictamen ministerial (impresiones).</summary>
    public string? Dictamen { get; set; }

    /// <summary>REGSOLAPA SMALLINT NOT NULL. Sin uso (confirmado 2026-06-12); se mantiene mapeada solo porque la columna es NOT NULL sin default — se persiste 0.</summary>
    public short RegSolapa { get; set; }

    /// <summary>DURACION VARCHAR(30): duración del plan en texto (constancias).</summary>
    public string? Duracion { get; set; }

    /// <summary>IDIOMA VARCHAR(50) NOT NULL: idioma de la carrera (constancias).</summary>
    public required string Idioma { get; set; }
}
