using Esba.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Esba.Web.Seguridad;

/// <summary>
/// Políticas de autorización (migration_improvements.md §2.7: el servidor deniega,
/// la UI solo oculta). Se evalúan sobre los claims de la cookie, sin ir a la base.
/// - Por perfil (USUARIOS.TIPO, 12.3.1): <see cref="Secretaria"/>, <see cref="Docentes"/>,
///   <see cref="Alumnos"/>. Cada carpeta de páginas declara la suya en su _Imports.razor.
/// - <see cref="Supervisores"/>: pantallas de Administración (SUPERV='S'; solo secretaría
///   puede ser supervisor, lo garantiza el validador del ABM).
/// La autorización fina por opción de menú/carrera (BARRA_SEGU → MNUOPC) es 12.3.
/// </summary>
public static class EsbaPolicies
{
    public const string Supervisores = "Supervisores";
    public const string Secretaria = "Secretaria";
    public const string Docentes = "Docentes";
    public const string Alumnos = "Alumnos";

    public static void Configurar(AuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddPolicy(Supervisores, policy =>
            policy.RequireClaim(EsbaClaims.Supervisor, "S"));

        // Secretaría se evalúa por extensión y no por RequireClaim: una cookie emitida
        // antes de que existiera el claim de tipo (usuarios ya logueados al desplegar)
        // no trae "esba:tipo", y para ellos el perfil es secretaría, como las filas
        // previas a la migración 2026-10-02.
        options.AddPolicy(Secretaria, policy =>
            policy.RequireAssertion(ctx => ctx.User.TipoDeUsuario() == TipoUsuario.Secretaria));

        options.AddPolicy(Docentes, policy =>
            policy.RequireClaim(EsbaClaims.Tipo, TipoUsuarioCodigo.Docente));

        options.AddPolicy(Alumnos, policy =>
            policy.RequireClaim(EsbaClaims.Tipo, TipoUsuarioCodigo.Alumno));
    }
}
