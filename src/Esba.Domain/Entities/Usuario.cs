namespace Esba.Domain.Entities;

/// <summary>
/// Tabla USUARIOS: usuarios del sistema. PK: CODUSU (trigger USUARIOS_BI0 con
/// GEN_ID(G_USUARIOS)). Contraseña dual mientras convivan web y escritorio
/// (decisión 2026-07-06): PASSWD conserva el cifrado reversible legacy que lee
/// el Delphi (sesion.pas + EncriptoCadena2) y NPASSWD guarda el hash PBKDF2
/// que usa el login web. Al retirar el escritorio se dropea PASSWD.
/// </summary>
public class Usuario
{
    /// <summary>CODUSU INTEGER, PK generada por trigger.</summary>
    public int Codigo { get; set; }

    /// <summary>NOMBRE VARCHAR(15) NOT NULL: nombre de login.</summary>
    public required string NombreUsuario { get; set; }

    /// <summary>
    /// PASSWD VARCHAR(60) NOT NULL. Cifrado reversible EncriptoCadena2 que valida
    /// el escritorio Delphi; el valor '/' indica contraseña blanqueada (junto con
    /// CAMPASS='S'). El lado .NET lo mantiene sincronizado al cambiar la
    /// contraseña, NUNCA lo pisa con un hash (rompería el login del escritorio).
    /// </summary>
    public required string PasswordLegacy { get; set; }

    /// <summary>
    /// NPASSWD VARCHAR(60): hash PBKDF2 ($E1$) del login web. NULL = usuario que
    /// todavía no entró por la web; su primer login lo puebla validando contra
    /// PASSWD. Se dropea PASSWD (no esta columna) al retirar el escritorio.
    /// </summary>
    public string? PasswordHashNuevo { get; set; }

    /// <summary>NOMUSU VARCHAR(50): nombres reales.</summary>
    public string? Nombres { get; set; }

    /// <summary>APELLIDO VARCHAR(50).</summary>
    public string? Apellido { get; set; }

    /// <summary>CARGO VARCHAR(30).</summary>
    public string? Cargo { get; set; }

    /// <summary>SUPERV CHAR(1) 'S'/'N' (trigger default 'N'): supervisor, ve todas las carreras sin filtro BARRA_SEGU.</summary>
    public bool EsSupervisor { get; set; }

    /// <summary>CAMPASS CHAR(1) 'S'/'N' (trigger default 'N'): debe cambiar la contraseña en el próximo login.</summary>
    public bool DebeCambiarPassword { get; set; }

    /// <summary>UID VARCHAR(50): identificador de sesión única (seciones.pas) — un nuevo login lo pisa y la sesión anterior queda inválida.</summary>
    public string? SesionUid { get; set; }

    /// <summary>IMGFIRMA VARCHAR(30): archivo de imagen de firma (CARPETA_FIRMAS legacy) para constancias.</summary>
    public string? ImagenFirma { get; set; }

    /// <summary>
    /// FECHA_BAJ DATE: baja lógica introducida por el lado .NET (hito 10.1a).
    /// NULL = usuario activo. El login .NET rechaza a los usuarios dados de baja.
    /// </summary>
    public DateOnly? FechaBaja { get; set; }

    /// <summary>Carreras/opciones habilitadas (BARRA_SEGU).</summary>
    public ICollection<PermisoUsuario> Permisos { get; set; } = [];

    /// <summary>true si el usuario está dado de baja (FECHA_BAJ no nula).</summary>
    public bool EstaDeBaja => FechaBaja is not null;
}
