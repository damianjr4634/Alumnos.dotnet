namespace Esba.Application.DTOs.Academica;

/// <summary>Baja de una comisión (sucesor del DELETE de cargacomisiones.EliminarClick).</summary>
public sealed record EliminarComisionCommand
{
    public required string CodigoCarrera { get; init; }

    public required short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }
}
