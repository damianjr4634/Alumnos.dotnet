using Esba.Domain.Examenes;

namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Modelo completo de un acta por comisión, listo para que el servicio de reporte
/// (PDF/Excel) lo maquete. Lo arma el handler agrupando las cursadas por comisión.
/// </summary>
public sealed record ActaComisionModel
{
    public required TipoActaComision Tipo { get; init; }

    public required string Titulo { get; init; }

    public required string CarreraLarga { get; init; }

    /// <summary>Cuatrimestre/año tal como lo tipeó el usuario (ej. "1/24").</summary>
    public required string CuatrimestreAnio { get; init; }

    public required bool MuestraCorrespondienteCuatrimestre { get; init; }

    /// <summary>Una sección (página) por comisión-materia, con sus alumnos.</summary>
    public required IReadOnlyList<ActaComisionSeccion> Secciones { get; init; }
}

/// <summary>Una comisión-materia del acta y los alumnos que la integran.</summary>
public sealed record ActaComisionSeccion
{
    public required ActaComisionCabeceraDto Cabecera { get; init; }

    public required IReadOnlyList<ActaAlumnoDto> Alumnos { get; init; }
}
