namespace Esba.Application.DTOs.Ministerio;

/// <summary>
/// Fila cruda de CURSADA ⨝ ALUMNOS (+ tutor responsable) para el padrón de comisiones
/// al Ministerio. Sucesor de los SELECT de ComisionesAlMinisterio.pas (SqlDatos de la
/// nómina impresa y SqlComi del export Excel, unificados: el segundo es un superconjunto
/// de columnas del primero). Las columnas codificadas (modalidad, turno, año, división,
/// condición, género, documento) se derivan en <c>CodificacionMinisterio</c>.
/// </summary>
public sealed record PadronMinisterioAlumnoDto
{
    public short Cutuco { get; init; }

    /// <summary>COD_ALU sin recortar: el documento se formatea por posiciones fijas.</summary>
    public required string CodigoAlumno { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    /// <summary>CONDICION de la cursada: CURSANDO o RECURSANDO.</summary>
    public string? Condicion { get; init; }

    /// <summary>SEXO: 'F'/'M'.</summary>
    public string? Sexo { get; init; }

    public DateOnly? FechaNacimiento { get; init; }

    /// <summary>NACIONAL: país de nacimiento.</summary>
    public string? Nacionalidad { get; init; }

    /// <summary>LUG_NAC: jurisdicción/provincia de nacimiento.</summary>
    public string? LugarNacimiento { get; init; }

    public string? Domicilio { get; init; }

    /// <summary>COD_POS NUMERIC(4,0).</summary>
    public int? CodigoPostal { get; init; }

    public string? Localidad { get; init; }

    /// <summary>CSECU: colegio secundario de origen (solo terciarias: "título con el que ingresa").</summary>
    public string? ColegioSecundario { get; init; }

    /// <summary>TSECU: título secundario (solo terciarias).</summary>
    public string? TituloSecundario { get; init; }

    /// <summary>TUTORES.FAPELLIDO del padre/madre/tutor (solo secundarias). null si no hay tutor cargado.</summary>
    public string? ApellidoTutor { get; init; }

    /// <summary>TUTORES.FNOMBRE del padre/madre/tutor (solo secundarias).</summary>
    public string? NombreTutor { get; init; }

    /// <summary>TUTORES.FDNI del padre/madre/tutor (solo secundarias).</summary>
    public int? DniTutor { get; init; }
}
