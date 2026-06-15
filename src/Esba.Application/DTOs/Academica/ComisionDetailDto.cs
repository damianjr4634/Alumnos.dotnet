namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Comisión completa para precargar el formulario de edición (sucesor del
/// ModificarClick de cargacomisiones.pas). El horario viene ya decodificado en
/// marcas por día.
/// </summary>
public sealed record ComisionDetailDto
{
    public required string CodigoCarrera { get; init; }

    public required short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public string? SiglaMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? CodigoProfesor { get; init; }

    public bool EsTitular { get; init; }

    public IReadOnlyList<HorarioDiaComision> Horario { get; init; } = [];
}
