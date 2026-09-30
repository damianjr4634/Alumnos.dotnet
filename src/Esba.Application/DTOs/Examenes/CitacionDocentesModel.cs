using Esba.Domain.Examenes;

namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Modelo de la citación a profesores listo para maquetar (sucesor de
/// Imp_Mesas_citacion): una carta por docente, impresa en original y duplicado.
/// </summary>
public sealed record CitacionDocentesModel
{
    public required DateOnly FechaEmision { get; init; }

    public required FirmanteCitacion Firmante { get; init; }

    /// <summary>Nombre de la autoridad que firma (RECTOR/SECRETARIA/DIRESTU de la carrera de firma).</summary>
    public string? NombreFirmante { get; init; }

    public required bool ConImagenFirma { get; init; }

    public required IReadOnlyList<CitacionDocenteSeccion> Docentes { get; init; }
}

/// <summary>Una carta: el docente y las mesas a las que está convocado, en orden de fecha y hora.</summary>
public sealed record CitacionDocenteSeccion
{
    public required string CodigoProfesor { get; init; }

    public string? Docente { get; init; }

    public required IReadOnlyList<MesaCitacionDto> Mesas { get; init; }
}
