namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Cabecera de cada sección de un acta por comisión: una comisión-materia de COMARM
/// que cumple el filtro. Sucesor de las filas de SqlComi en lstactasARegular.pas /
/// lstactasreincorporacion.pas / lstactasexamenes.pas.
/// </summary>
public sealed record ActaComisionCabeceraDto
{
    public short Cutuco { get; init; }

    public required string CodigoMateria { get; init; }

    public string? DescripcionMateria { get; init; }

    public string? Docente { get; init; }
}
