namespace Esba.Application.DTOs.Certificados;

/// <summary>
/// Una línea del cuerpo de la equivalencia bachiller, en disposición a dos columnas
/// (sucesora de las filas COLUMNA1/COLUMNA2 de <c>XXX_IMPRESION_EQ_BAC</c>). El SP
/// reparte el listado de materias de la carrera en dos columnas físicas: las primeras
/// 20 líneas en <see cref="Columna1"/> y su continuación en <see cref="Columna2"/>.
/// Cada texto ya viene formateado ("SI &gt;&gt; Materia", "-- &gt;&gt; Materia" o el
/// separador de cuatrimestre); el relleno de la segunda columna se descarta en la maqueta.
/// </summary>
public sealed record LineaEquivalenciaBachillerDto
{
    public string? Columna1 { get; init; }

    public string? Columna2 { get; init; }
}
