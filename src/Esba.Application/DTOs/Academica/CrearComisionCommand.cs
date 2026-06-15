namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Alta de una comisión armada (sucesor del INSERT de
/// cargacomisiones.GrabaMateriaClick). El cuatrimestre del CUTUCO debe coincidir
/// con el de la materia y el horario no debe superponerse: lo valida el SP
/// XXX_VAL_COMISION tras insertar (en la misma transacción).
/// </summary>
public sealed record CrearComisionCommand : IComisionCampos
{
    public required string CodigoCarrera { get; init; }

    /// <summary>CUTUCO (3 díg.): cuatrimestre + turno + comisión.</summary>
    public required short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    /// <summary>CUA_ANIO ("124" = 1/24).</summary>
    public required string CuatrimestreAnio { get; init; }

    public string? CodigoProfesor { get; init; }

    /// <summary>true = titular ('T'), false = suplente ('S').</summary>
    public bool EsTitular { get; init; } = true;

    public IReadOnlyList<HorarioDiaComision> Horario { get; init; } = [];
}
