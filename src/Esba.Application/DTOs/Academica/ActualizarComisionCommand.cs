namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Modificación de una comisión (sucesor del UPDATE de
/// cargacomisiones.GrabaMateriaClick). La clave (carrera, CUTUCO, materia,
/// cuat/año) identifica la fila y no cambia; se editan docente, horario y
/// titular/suplente. Tras grabar se revalida con XXX_VAL_COMISION.
/// </summary>
public sealed record ActualizarComisionCommand : IComisionCampos
{
    public required string CodigoCarrera { get; init; }

    public required short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? CodigoProfesor { get; init; }

    public bool EsTitular { get; init; } = true;

    public IReadOnlyList<HorarioDiaComision> Horario { get; init; } = [];
}
