using Esba.Domain.Enums;

namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Alta de usuario (sucesor del INSERT de AltaUsuario.pas). CODUSU lo genera el
/// trigger. La contraseña viaja en claro hasta el caso de uso, que la hashea
/// (PBKDF2, migration_improvements.md §2.7) — nunca se persiste en claro. El
/// alta nace con CAMPASS='S' (cambio forzado en el primer login): el admin pone
/// una clave inicial y el usuario la cambia (mejora sobre el legacy, que nacía 'N').
/// Desde 2026-10-02 el que da de alta elige el <see cref="Tipo"/> y a quién queda
/// atado el usuario (docente o alumno); el validador exige el vínculo según el tipo.
/// </summary>
public sealed record CrearUsuarioCommand
{
    public required string NombreUsuario { get; init; }

    public required string Password { get; init; }

    public string? Nombres { get; init; }

    public string? Apellido { get; init; }

    public string? Cargo { get; init; }

    /// <summary>SUPERV: ve todas las carreras sin filtro BARRA_SEGU y administra el sistema. Solo para secretaría.</summary>
    public bool EsSupervisor { get; init; }

    /// <summary>Perfil de acceso (USUARIOS.TIPO). Default: secretaría, como todo usuario histórico.</summary>
    public TipoUsuario Tipo { get; init; } = TipoUsuario.Secretaria;

    /// <summary>DOCENTES.CODPROFES: obligatorio si <see cref="Tipo"/> es Docente.</summary>
    public string? CodigoDocente { get; init; }

    /// <summary>ALUMNOS.CARRE: obligatoria si <see cref="Tipo"/> es Alumno.</summary>
    public string? AlumnoCarrera { get; init; }

    /// <summary>ALUMNOS.COD_ALU: obligatorio si <see cref="Tipo"/> es Alumno.</summary>
    public string? AlumnoCodigo { get; init; }
}
