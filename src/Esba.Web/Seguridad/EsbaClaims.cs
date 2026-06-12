using System.Security.Claims;
using Esba.Application.DTOs.Administracion;
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
        };
        claims.AddRange(sesion.Permisos.Select(p => new Claim(Permiso, p)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static bool EsSupervisor(this ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(Supervisor) == "S";

    public static int? CodigoUsuario(this ClaimsPrincipal usuario) =>
        int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var codigo) ? codigo : null;

    public static IReadOnlyList<string> Permisos(this ClaimsPrincipal usuario) =>
        usuario.FindAll(Permiso).Select(c => c.Value).ToList();
}
