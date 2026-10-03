using Esba.Domain.Enums;

namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Resultado de un login exitoso: lo que la capa web convierte en claims del
/// ClaimsPrincipal (sucesor de las globals CodUsu/Superv de FuncionesConfiguracion).
/// </summary>
public sealed record SesionIniciadaDto
{
    public required int CodigoUsuario { get; init; }

    public required string NombreUsuario { get; init; }

    public string? NombreCompleto { get; init; }

    public bool EsSupervisor { get; init; }

    /// <summary>CAMPASS legacy: la UI debe forzar el cambio de contraseña antes de continuar.</summary>
    public bool DebeCambiarPassword { get; init; }

    /// <summary>UID de sesión única (seciones.pas): el login regenera este valor e invalida la sesión anterior.</summary>
    public required string SesionUid { get; init; }

    /// <summary>Códigos de carrera/opción habilitados (BARRA_SEGU) para las políticas de autorización.</summary>
    public required IReadOnlyList<string> Permisos { get; init; }

    /// <summary>Perfil de acceso (USUARIOS.TIPO): decide el área de la aplicación y las políticas (12.3 ampliado).</summary>
    public TipoUsuario Tipo { get; init; } = TipoUsuario.Secretaria;

    /// <summary>CODPROFES del docente vinculado (solo tipo Docente): alcance de datos de su área.</summary>
    public string? CodigoDocente { get; init; }

    /// <summary>Carrera del alumno vinculado (solo tipo Alumno).</summary>
    public string? AlumnoCarrera { get; init; }

    /// <summary>Código del alumno vinculado (solo tipo Alumno).</summary>
    public string? AlumnoCodigo { get; init; }
}
