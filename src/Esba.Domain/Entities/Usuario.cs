namespace Esba.Domain.Entities;

/// <summary>
/// Tabla USUARIOS: usuarios del sistema. PK: CODUSU (trigger USUARIOS_BI0 con
/// GEN_ID(G_USUARIOS)). El login legacy (sesion.pas) compara la contraseña
/// descifrando PASSWD con EncriptoCadena2 (cifrado reversible): el nuevo
/// sistema re-hashea en el primer login exitoso (migration_improvements.md §2.7).
/// </summary>
public class Usuario
{
    /// <summary>CODUSU INTEGER, PK generada por trigger.</summary>
    public int Codigo { get; set; }

    /// <summary>NOMBRE VARCHAR(15) NOT NULL: nombre de login.</summary>
    public required string NombreUsuario { get; set; }

    /// <summary>
    /// PASSWD VARCHAR(60) NOT NULL. Legacy: cifrado reversible EncriptoCadena2;
    /// el valor '/' indica contraseña blanqueada (junto con CAMPASS='S').
    /// Destino: hash PBKDF2 con re-hash en el primer login. // TODO-migrar re-hash
    /// </summary>
    public required string PasswordHash { get; set; }

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

    /// <summary>Carreras/opciones habilitadas (BARRA_SEGU).</summary>
    public ICollection<PermisoUsuario> Permisos { get; set; } = [];
}
