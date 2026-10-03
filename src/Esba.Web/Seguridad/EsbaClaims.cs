using System.Security.Claims;
using Esba.Application.DTOs.Administracion;
using Esba.Domain.Enums;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Esba.Web.Seguridad;

/// <summary>
/// Claims del usuario logueado: sucesores de las variables globales CodUsu /
/// Superv de FuncionesConfiguracion y de los permisos BARRA_SEGU
/// (migration_improvements.md §2.3.3 y §2.7).
/// </summary>
public static class EsbaClaims
{
    public const string Supervisor = "esba:superv";
    public const string SesionUid = "esba:sesion-uid";
    public const string Permiso = "esba:permiso";
    public const string DebeCambiarPassword = "esba:campass";
    public const string NombreCompleto = "esba:nombre-completo";

    /// <summary>Perfil de acceso (USUARIOS.TIPO): 'SEC'/'DOC'/'ALU'. Base de las policies por área (12.3 ampliado).</summary>
    public const string Tipo = "esba:tipo";

    /// <summary>CODPROFES del docente vinculado: alcance de datos del área docente.</summary>
    public const string Docente = "esba:docente";

    /// <summary>Alumno vinculado como "CARRE|COD_ALU" (portal futuro).</summary>
    public const string Alumno = "esba:alumno";

    public static ClaimsPrincipal CrearPrincipal(SesionIniciadaDto sesion)
    {
        ArgumentNullException.ThrowIfNull(sesion);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sesion.CodigoUsuario.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, sesion.NombreUsuario),
            new(NombreCompleto, sesion.NombreCompleto ?? sesion.NombreUsuario),
            new(Supervisor, sesion.EsSupervisor ? "S" : "N"),
            new(DebeCambiarPassword, sesion.DebeCambiarPassword ? "S" : "N"),
            new(SesionUid, sesion.SesionUid),
            new(Tipo, TipoUsuarioCodigo.ACodigo(sesion.Tipo)),
        };
        claims.AddRange(sesion.Permisos.Select(p => new Claim(Permiso, p)));

        if (sesion.CodigoDocente is not null)
        {
            claims.Add(new Claim(Docente, sesion.CodigoDocente));
        }

        if (sesion.AlumnoCarrera is not null && sesion.AlumnoCodigo is not null)
        {
            claims.Add(new Claim(Alumno, $"{sesion.AlumnoCarrera}|{sesion.AlumnoCodigo}"));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static bool EsSupervisor(this ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(Supervisor) == "S";

    /// <summary>Perfil del usuario logueado. Sin claim (cookie emitida antes del cambio) se asume secretaría, como las filas previas a la migración.</summary>
    public static TipoUsuario TipoDeUsuario(this ClaimsPrincipal usuario) =>
        TipoUsuarioCodigo.DesdeCodigo(usuario.FindFirstValue(Tipo));

    /// <summary>CODPROFES del docente vinculado; null si el usuario no es docente.</summary>
    public static string? CodigoDocente(this ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(Docente);

    /// <summary>
    /// Inicio de cada perfil: "/" (buscador de alumnos) para secretaría, "/docente" para
    /// docentes. Único lugar que lo decide: lo usan el login, el cambio de contraseña
    /// forzado, la página de acceso denegado y la de error. Mandar a "/" a un docente
    /// termina en acceso denegado (la home de secretaría exige su policy).
    /// </summary>
    public static string RutaInicio(TipoUsuario tipo) => tipo == TipoUsuario.Docente ? "/docente" : "/";

    public static string RutaInicio(this ClaimsPrincipal usuario) => RutaInicio(usuario.TipoDeUsuario());

    public static int? CodigoUsuario(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var codigo) ? codigo : null;

    public static IReadOnlyList<string> Permisos(this ClaimsPrincipal usuario) =>
        usuario.FindAll(Permiso).Select(c => c.Value).ToList();
}
