namespace Esba.Application.DTOs.Ministerio;

/// <summary>
/// Fila ya codificada del padrón que se exporta al Ministerio: superconjunto de las
/// columnas de los dos layouts legacy (terciario y secundario) de
/// ComisionesAlMinisterio.pas. El servicio Excel elige qué columnas volcar según
/// <c>PadronMinisterioModel.EsTerciaria</c>. Las columnas que el legacy exportaba
/// siempre vacías (PISO, COMUNA, PARTIDO, PROVINCIA, PPI 1–3) no se modelan: el
/// servicio las escribe en blanco.
/// </summary>
public sealed record FilaPadronMinisterioDto
{
    public short Cutuco { get; init; }

    /// <summary>NOMHOJA: "CARRE-CUTUCO", nombre de la hoja cuando se exporta una comisión por hoja.</summary>
    public required string NombreHoja { get; init; }

    /// <summary>NÚMERO DE RESOLUCION DEL PLAN.</summary>
    public string? Resolucion { get; init; }

    /// <summary>ORIENTACION (terciarias): 'TC' tecnicatura.</summary>
    public string? Orientacion { get; init; }

    /// <summary>TIPO DE CARRERA (terciarias): 'G' grado.</summary>
    public string? TipoCarrera { get; init; }

    /// <summary>MODALIDAD: P/D (terciarias) o C/A (secundarias).</summary>
    public string? Modalidad { get; init; }

    /// <summary>TURNO: TM/TT/TV/TN o TD.</summary>
    public string? Turno { get; init; }

    /// <summary>AÑO DE ESTUDIO.</summary>
    public int? AnioEstudio { get; init; }

    /// <summary>CUATRIMESTRE (terciarias).</summary>
    public int? Cuatrimestre { get; init; }

    /// <summary>DIVISION: A–F.</summary>
    public string? Division { get; init; }

    public string? Apellido { get; init; }

    public string? Nombre { get; init; }

    /// <summary>TIPO DE DOCUMENTO: DNI, CI, LE, PAS.</summary>
    public required string TipoDocumento { get; init; }

    /// <summary>NRO. DOCUMENTO con puntos.</summary>
    public required string NumeroDocumento { get; init; }

    /// <summary>GENERO: VARON/MUJER.</summary>
    public required string Genero { get; init; }

    public DateOnly? FechaNacimiento { get; init; }

    /// <summary>PAIS DE NACIMIENTO (NACIONAL).</summary>
    public string? PaisNacimiento { get; init; }

    /// <summary>JURISDICCION/PROVINCIA DE NACIMIENTO (LUG_NAC).</summary>
    public string? LugarNacimiento { get; init; }

    public string? Domicilio { get; init; }

    public int? CodigoPostal { get; init; }

    /// <summary>BARRIO/LOCALIDAD (LOCALI).</summary>
    public string? Localidad { get; init; }

    /// <summary>CONDICION (terciarias): R regular / RC recursante.</summary>
    public string? Condicion { get; init; }

    /// <summary>TITULO CON EL QUE INGRESA (terciarias): colegio + título secundario.</summary>
    public string? TituloIngreso { get; init; }

    /// <summary>APELLIDO ADULTO RESPONSABLE (secundarias).</summary>
    public string? ApellidoTutor { get; init; }

    /// <summary>NOMBRE ADULTO RESPONSABLE (secundarias).</summary>
    public string? NombreTutor { get; init; }

    /// <summary>TIPO DOCUMENTO DEL ADULTO RESPONSABLE (secundarias): 'DNI' si hay tutor.</summary>
    public string? TipoDocumentoTutor { get; init; }

    /// <summary>NRO. DOCUMENTO DEL ADULTO RESPONSABLE (secundarias).</summary>
    public int? DniTutor { get; init; }
}
