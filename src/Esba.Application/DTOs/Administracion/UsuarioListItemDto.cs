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

    public bool EstaDeBaja => FechaBaja is not null;
}
