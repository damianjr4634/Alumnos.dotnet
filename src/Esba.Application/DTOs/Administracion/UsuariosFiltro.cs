namespace Esba.Application.DTOs.Administracion;

/// <summary>
/// Filtro + paginación del listado de usuarios (sucesor de AltaUsuario/BajaUsuarios),
/// server-side (§3.2).
/// </summary>
public sealed record UsuariosFiltro
{
    /// <summary>Texto libre sobre nombre de login, nombres, apellido o cargo.</summary>
    public string? Texto { get; init; }

    /// <summary>Si es false (default), solo trae usuarios activos (FECHA_BAJ nula).</summary>
    public bool IncluirBajas { get; init; }

    public string? OrdenarPor { get; init; }

    public bool Descendente { get; init; }

    public int Skip { get; init; }

    public int Take { get; init; } = 25;
}
