using Esba.Domain.Asistencias;

namespace Esba.Application.DTOs.Asistencias;

/// <summary>
/// Modelo completo de una carpeta por comisión (asistencia o trabajos prácticos),
/// listo para que el servicio de reporte lo maquete. Lo arma el handler agrupando
/// las cursadas por comisión-materia.
/// </summary>
public sealed record CarpetaComisionModel
{
    public required TipoCarpetaComision Tipo { get; init; }

    public required string CarreraLarga { get; init; }

    /// <summary>Cuatrimestre/año tal como lo tipeó el usuario (ej. "1/24").</summary>
    public required string CuatrimestreAnio { get; init; }

    /// <summary>Fecha de emisión: el legacy imprimía "EMISION: fecha" y el ciclo lectivo del año en curso.</summary>
    public required DateOnly FechaEmision { get; init; }

    /// <summary>Una hoja por comisión-materia, con sus alumnos.</summary>
    public required IReadOnlyList<CarpetaComisionSeccion> Secciones { get; init; }
}

/// <summary>Una comisión-materia de la carpeta, con cursantes y recursantes separados.</summary>
public sealed record CarpetaComisionSeccion
{
    public required CarpetaComisionCabeceraDto Cabecera { get; init; }

    public required IReadOnlyList<CarpetaComisionAlumnoDto> Cursando { get; init; }

    /// <summary>Recursantes: van al pie de la hoja bajo el subtítulo RECURSANTES, numerados aparte.</summary>
    public required IReadOnlyList<CarpetaComisionAlumnoDto> Recursantes { get; init; }
}
