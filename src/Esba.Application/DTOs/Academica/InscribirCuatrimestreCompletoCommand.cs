namespace Esba.Application.DTOs.Academica;

/// <summary>
/// Inscripción masiva: inscribe al alumno en todas las materias del cuatrimestre
/// del curso elegido (sucesor del "Conjunto" de inscripción que invoca al SP
/// XXX_INSC_CUAT_16032023). El cuatrimestre lo determina el primer dígito del
/// curso; el SP recorre las materias de ese cuatrimestre y valida cada una.
/// </summary>
public sealed record InscribirCuatrimestreCompletoCommand
{
    public required string CodigoCarrera { get; init; }

    public required string CodigoAlumno { get; init; }

    /// <summary>CURSO = CUTUCO (3 díg.): cuatrimestre + turno + comisión.</summary>
    public required short Curso { get; init; }

    /// <summary>CUA_ANIO ("124" = 1/24).</summary>
    public required string CuatrimestreAnio { get; init; }

    /// <summary>CodUsu del usuario logueado (el SP usa su flag superv).</summary>
    public required int CodigoUsuario { get; init; }
}
