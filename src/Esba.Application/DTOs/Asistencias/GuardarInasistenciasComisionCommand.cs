namespace Esba.Application.DTOs.Asistencias;

/// <summary>Una inasistencia a guardar (alumno + fecha + tipo).</summary>
public sealed record FaltaInasistencia
{
    public required string CodigoAlumno { get; init; }

    public required DateOnly Fecha { get; init; }

    public required string CodigoFalta { get; init; }

    public decimal Cantidad { get; init; }
}

/// <summary>
/// Guarda las inasistencias de una comisión (sucesor de
/// CargaInasistenciasComisionNuevo.GrabamesaClick). Reemplaza el conjunto de
/// faltas de la comisión para el año del cuatrimestre por <see cref="Faltas"/>:
/// lo que no esté en la lista se borra. Acotado por (carrera, cutuco, materia,
/// año) — el CUTUCO ya identifica el cuatrimestre, así que no hace falta el
/// rango de meses ni el chequeo de modalidad por carrera del legacy.
/// </summary>
public sealed record GuardarInasistenciasComisionCommand
{
    public required string CodigoCarrera { get; init; }

    public required short Cutuco { get; init; }

    public required string CuatrimestreAnio { get; init; }

    /// <summary>Materia; null en carreras trimestrales que cargan por comisión sin materia.</summary>
    public string? CodigoMateria { get; init; }

    public required int CodigoUsuario { get; init; }

    public IReadOnlyList<FaltaInasistencia> Faltas { get; init; } = [];
}
