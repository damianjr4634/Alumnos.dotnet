namespace Esba.Application.DTOs.Asistencias;

/// <summary>
/// Cabecera de cada hoja de una carpeta por comisión: una comisión-materia de COMARM.
/// Sucesor de las filas de SqlComi en lstplanasis.pas / lstNotasyPractico.pas (a
/// diferencia de las actas, acá se trae si el docente es titular o suplente, TIT_SUP).
/// </summary>
public sealed record CarpetaComisionCabeceraDto
{
    public short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public string? DescripcionMateria { get; init; }

    public string? Docente { get; init; }

    /// <summary>TIT_SUP de COMARM: 'T' titular, cualquier otro valor se imprime como suplente.</summary>
    public string? TitularSuplente { get; init; }
}
