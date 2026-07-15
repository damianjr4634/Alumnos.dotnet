using Esba.Domain.Asistencias;

namespace Esba.Application.DTOs.Asistencias;

/// <summary>
/// Filtros de las carpetas por comisión (planillas en blanco que completa el docente:
/// asistencia o trabajos prácticos). Sucesor de los controles de lstplanasis.pas y
/// lstNotasyPractico.pas (comisión + cuatrimestre + materia opcional); la carrera
/// viene del contexto, no de un global.
/// </summary>
public sealed record GenerarCarpetaComisionCommand
{
    public required TipoCarpetaComision Tipo { get; init; }

    public required string CodigoCarrera { get; init; }

    /// <summary>CUA_ANIO en formato "d/aa" (lo que tipea el usuario, ej. "1/24").</summary>
    public required string CuatrimestreAnio { get; init; }

    /// <summary>Comisión (CUTUCO). Opcional: vacío lista todas las comisiones del cuatrimestre.</summary>
    public short? Cutuco { get; init; }

    /// <summary>Código de materia. Opcional: vacío lista todas las materias.</summary>
    public string? CodigoMateria { get; init; }
}
