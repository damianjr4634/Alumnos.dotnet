using Esba.Domain.Examenes;

namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Filtros y opciones de la citación a profesores (sucesor del panel de parámetros de
/// dxBarButton20Click en FrmEsba.pas: docente desde/hasta, fecha desde/hasta, carreras y
/// firma; la opción "Imagen Firma" del legacy ya no existe, la imagen acompaña siempre al
/// nombre). Las autoridades firmantes salen de la carrera indicada en
/// <see cref="CodigoCarreraFirma"/> (el legacy usaba las de la carrera activa, un global).
/// </summary>
public sealed record GenerarCitacionDocentesCommand
{
    /// <summary>Rango de códigos de docente (CODPROFES). Ambos o ninguno.</summary>
    public string? CodigoProfesorDesde { get; init; }

    public string? CodigoProfesorHasta { get; init; }

    public required DateOnly FechaDesde { get; init; }

    public required DateOnly FechaHasta { get; init; }

    /// <summary>Carreras a incluir. Vacío = todas.</summary>
    public IReadOnlyList<string> CodigosCarrera { get; init; } = [];

    /// <summary>Autoridad que firma; su imagen de firma sale siempre que esté configurada (regla 2026-10-01).</summary>
    public required FirmanteCitacion Firmante { get; init; }

    /// <summary>Carrera de la que se toman el nombre del rector/a, secretaria o director/a de estudios.</summary>
    public required string CodigoCarreraFirma { get; init; }
}
