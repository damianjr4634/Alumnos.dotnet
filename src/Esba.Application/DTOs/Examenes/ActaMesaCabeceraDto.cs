namespace Esba.Application.DTOs.Examenes;

/// <summary>
/// Cabecera del acta volante de una mesa de examen. Sucesor de la fila de SqlComi
/// en lstactasMesas.pas (MESAS + DOCENTES + MATERIAS por mesa y carrera).
/// </summary>
public sealed record ActaMesaCabeceraDto
{
    /// <summary>Titular y vocales concatenados ("TITULAR - VOCAL1 - VOCAL2").</summary>
    public string? Docente { get; init; }

    /// <summary>COMI1 de la mesa, usado como cutuco para el encabezado.</summary>
    public int? Cutuco { get; init; }

    public string? DescripcionMateria { get; init; }

    public int Dia { get; init; }

    public int Mes { get; init; }

    public int Anio { get; init; }

    /// <summary>MATERIAS.CUATRIM, usado como respaldo cuando el COMI1 no es decodificable.</summary>
    public int? CuatrimestreMateria { get; init; }
}
