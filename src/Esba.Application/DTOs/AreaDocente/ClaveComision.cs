namespace Esba.Application.DTOs.AreaDocente;

/// <summary>Clave natural de una comisión armada (PK de COMARM).</summary>
public sealed record ClaveComision
{
    public required string CodigoCarrera { get; init; }

    public short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }
}

/// <summary>Clave natural de una mesa de examen (PK de MESAS).</summary>
public sealed record ClaveMesa
{
    public required string CodigoCarrera { get; init; }

    public int NumeroMesa { get; init; }
}

/// <summary>
/// Quién ejecuta una acción sobre una precarga docente, armado desde los claims del
/// usuario logueado (nunca desde la UI): secretaría puede siempre; un docente solo
/// sobre sus comisiones/mesas (titular) y mientras la carga está en borrador.
/// </summary>
public sealed record ActorCargaDocente
{
    public required int CodigoUsuario { get; init; }

    /// <summary>USUARIOS.TIPO = SEC.</summary>
    public bool EsSecretaria { get; init; }

    /// <summary>CODPROFES del docente logueado (claim esba:docente); null para secretaría.</summary>
    public string? CodigoDocente { get; init; }
}
