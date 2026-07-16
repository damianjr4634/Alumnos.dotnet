namespace Esba.Application.DTOs.Asistencias;

/// <summary>
/// Resultado del export Excel de carpetas por comisión. Como el legacy generaba un
/// archivo por comisión/materia (Notas_{CUTUCO}_{materia}.xls) y por HTTP se descarga
/// uno solo, con varias comisiones el contenido es un .zip con un .xlsx por cada una;
/// con una sola, el .xlsx directo.
/// </summary>
public sealed record CarpetaComisionExcelResultado
{
    public required byte[] Contenido { get; init; }

    /// <summary>Nombre de descarga sugerido (con extensión .xlsx o .zip).</summary>
    public required string NombreArchivo { get; init; }

    public required bool EsZip { get; init; }
}
