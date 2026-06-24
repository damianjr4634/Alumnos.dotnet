namespace Esba.Domain.Certificados;

/// <summary>
/// Datos de entrada para componer el cuerpo de la Constancia de Alumno Regular
/// (sucesor de los campos del formulario constanciaalumnoregular.pas). Los arma el
/// caso de uso a partir de la cursada vigente y los datos de la carrera; el
/// formateo de texto es puro (sin I/O) y vive en <see cref="ConstanciaRegularFormatter"/>.
/// </summary>
public sealed record ConstanciaRegularContexto
{
    /// <summary>Apellido y nombre del alumno (se imprime tal cual).</summary>
    public required string NombreCompleto { get; init; }

    /// <summary>Código de alumno con separador de miles (PonePuntos).</summary>
    public required string CodigoConPuntos { get; init; }

    /// <summary>Nombre de la carrera (se imprime en mayúsculas).</summary>
    public required string NombreCarrera { get; init; }

    /// <summary>Código de la carrera (BAC / 333 / 650 / …): define la tabla de horarios.</summary>
    public required string CodigoCarrera { get; init; }

    /// <summary>true si la modalidad es a distancia (DISTANCIA = 'S').</summary>
    public bool EsADistancia { get; init; }

    /// <summary>Número de cuatrimestre, primer dígito del CUTUCO (1 = primer, 2 = segundo…).</summary>
    public int Cuatrimestre { get; init; }

    /// <summary>Turno, segundo dígito del CUTUCO (1–4): define el horario de cursada.</summary>
    public int Turno { get; init; }

    /// <summary>true para carreras anuales (333/650): el texto dice "año" en vez de "cuatrimestre".</summary>
    public bool EsCarreraPorAnio { get; init; }

    /// <summary>Número de dictamen del Consejo Federal (solo modalidad a distancia).</summary>
    public string? Dictamen { get; init; }

    /// <summary>Ante quién se presenta la constancia (se imprime en mayúsculas).</summary>
    public required string AnteQuien { get; init; }

    /// <summary>Fecha de emisión (se imprime "A los D días del mes de M de A").</summary>
    public DateOnly Fecha { get; init; }
}
