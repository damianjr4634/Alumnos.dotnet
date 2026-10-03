using Esba.Domain.Enums;

namespace Esba.Application.DTOs.Administracion;

/// <summary>Fila de la grilla de usuarios (sucesor de AltaUsuario/BajaUsuarios).</summary>
public sealed record UsuarioListItemDto
{
    public required int Codigo { get; init; }

    public required string NombreUsuario { get; init; }

    public string? Nombres { get; init; }

    public string? Apellido { get; init; }

    public string? Cargo { get; init; }

    public bool EsSupervisor { get; init; }

    public bool DebeCambiarPassword { get; init; }

    /// <summary>FECHA_BAJ: NULL = activo. Cuando tiene valor, el usuario está dado de baja.</summary>
    public DateOnly? FechaBaja { get; init; }

    /// <summary>Perfil de acceso (USUARIOS.TIPO).</summary>
    public TipoUsuario Tipo { get; init; }

    /// <summary>CODPROFES del docente vinculado (solo tipo Docente).</summary>
    public string? CodigoDocente { get; init; }

    /// <summary>DOCENTES.DOCENTE del vinculado, para mostrar en la grilla; null si no hay vínculo o el docente no existe.</summary>
    public string? NombreDocente { get; init; }

    /// <summary>Carrera del alumno vinculado (solo tipo Alumno).</summary>
    public string? AlumnoCarrera { get; init; }

    /// <summary>Código del alumno vinculado (solo tipo Alumno).</summary>
    public string? AlumnoCodigo { get; init; }

    public bool EstaDeBaja => FechaBaja is not null;

    /// <summary>Texto del vínculo para la grilla: "017 — Pérez, Juan" para docentes, "TER-12345678" para alumnos, vacío para secretaría.</summary>
    public string Vinculo => Tipo switch
    {
        TipoUsuario.Docente when CodigoDocente is not null =>
            NombreDocente is null ? CodigoDocente : $"{CodigoDocente} — {NombreDocente}",
        TipoUsuario.Alumno when AlumnoCodigo is not null => $"{AlumnoCarrera}-{AlumnoCodigo}",
        _ => string.Empty,
    };
}
