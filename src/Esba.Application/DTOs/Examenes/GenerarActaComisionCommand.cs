using Esba.Domain.Examenes;

namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Filtros para generar un acta por comisión (A/REGULAR, Reincorporación o Exámenes).
/// Sucesor de los controles de lstactasARegular.pas (comisión + cuatrimestre +
/// materia opcional); la carrera viene del contexto del menú, no de un global.
/// </summary>
public sealed record GenerarActaComisionCommand
{
    public required TipoActaComision Tipo { get; init; }

    public required string CodigoCarrera { get; init; }

    /// <summary>CUA_ANIO en formato "d/aa" (lo que tipea el usuario, ej. "1/24").</summary>
    public required string CuatrimestreAnio { get; init; }

    /// <summary>Comisión (CUTUCO). Opcional: vacío lista todas las comisiones del cuatrimestre.</summary>
    public short? Cutuco { get; init; }

    /// <summary>Código de materia. Opcional: vacío lista todas las materias.</summary>
    public string? CodigoMateria { get; init; }
}
