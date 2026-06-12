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
}
