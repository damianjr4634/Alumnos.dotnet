namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Filtros para generar el acta volante de una mesa de examen. Sucesor de los
/// controles de lstactasMesas.pas (mesa + tipo de examen); la carrera viene del
/// contexto del menú.
/// </summary>
public sealed record GenerarActaMesaCommand
{
    public required string CodigoCarrera { get; init; }

    public required int Mesa { get; init; }

    /// <summary>
    /// Tipo de examen pasado a XXX_MESAS_ALUMNOS: FINAL (default) o, para BAC/333/650,
    /// LIBRES/PREVIOS/DICIEMBRE/MARZO/P/EQUIVALEN.
    /// </summary>
    public required string TipoExamen { get; init; }
}
