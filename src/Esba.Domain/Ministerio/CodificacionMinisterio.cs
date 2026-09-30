using Esba.Domain.Examenes;

namespace Esba.Domain.Ministerio;

/// <summary>
/// Codificación de los datos del padrón de comisiones que se informa al Ministerio de
/// Educación. Sucesor de las expresiones SQL que ComisionesAlMinisterio.pas
/// (<c>BitBtn1Click</c>) armaba por concatenación: modalidad, turno, año de estudio,
/// división, condición, género y formato de documento. Funciones puras sin
/// dependencias (§2.1.3), cubiertas por tests unitarios.
/// </summary>
/// <remarks>
/// Diferencias deliberadas con el legacy: la división se decodifica con el mapa
/// confirmado de <see cref="CodigoComision"/> (1=A … 6=F; el SQL legacy cortaba en D y
/// devolvía NULL para la 5ª y 6ª comisión), y la edad se calcula por calendario (el
/// legacy hacía <c>TRUNC((CURRENT_DATE-FEC_NAC)/365)</c>, que se corre un día por cada
/// bisiesto vivido).
/// </remarks>
public static class CodificacionMinisterio
{
    /// <summary>TIPOCARRE del padrón terciario: siempre 'G' (grado).</summary>
    public const string TipoCarreraGrado = "G";

    /// <summary>Código de la carrera secundaria cuyo CUTUCO codifica el año directamente (no por pares de cuatrimestres).</summary>
    public const string CarreraSecundarioPorAnio = "650";

    private const string TipoTerciaria = "TER";
    private const string TipoBachillerato = "BAC";
    private const int LongitudTipoDocumento = 3;
    private const int LongitudCodigoAlumno = 11;

    /// <summary>true si CARRERA.TIPO es 'TER': el padrón usa el layout terciario.</summary>
    public static bool EsTerciaria(string? tipoCarrera) =>
        string.Equals(tipoCarrera?.Trim(), TipoTerciaria, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// MODALIDAD: terciarias 'P' presencial / 'D' a distancia (CARRERA.DISTANCIA);
    /// secundarias 'C' común (TIPO='BAC') / 'A' adultos.
    /// </summary>
    public static string Modalidad(string? tipoCarrera, bool esADistancia)
    {
        if (EsTerciaria(tipoCarrera))
        {
            return esADistancia ? "D" : "P";
        }

        return string.Equals(tipoCarrera?.Trim(), TipoBachillerato, StringComparison.OrdinalIgnoreCase) ? "C" : "A";
    }

    /// <summary>ORIENTACION (solo terciarias): 'TC' si la carrera es una tecnicatura; si no, null.</summary>
    public static string? Orientacion(string? descripcionCarrera) =>
        (descripcionCarrera ?? string.Empty).TrimStart().StartsWith("TECNICATURA", StringComparison.OrdinalIgnoreCase)
            ? "TC"
            : null;

    /// <summary>
    /// TURNO: 'TD' para carreras a distancia; si no, por el 2º dígito del CUTUCO
    /// (1=TM mañana, 2=TT tarde, 3=TV vespertino, 4=TN noche). null si no se puede decodificar.
    /// </summary>
    public static string? Turno(int cutuco, bool esADistancia)
    {
        if (esADistancia)
        {
            return "TD";
        }

        if (!CodigoComision.TryDescomponer(cutuco, out var codigo))
        {
            return null;
        }

        return codigo.Turno switch
        {
            1 => "TM",
            2 => "TT",
            3 => "TV",
            4 => "TN",
            _ => null,
        };
    }

    /// <summary>
    /// AÑO DE ESTUDIO: por pares de cuatrimestres (1–2 → 1, 3–4 → 2, 5–6 → 3) salvo la
    /// carrera 650, cuyo primer dígito del CUTUCO ya es el año. null si no se puede decodificar.
    /// </summary>
    public static int? AnioEstudio(int cutuco, string? codigoCarrera)
    {
        if (!CodigoComision.TryDescomponer(cutuco, out var codigo))
        {
            return null;
        }

        if (string.Equals(codigoCarrera?.Trim(), CarreraSecundarioPorAnio, StringComparison.Ordinal))
        {
            return codigo.Cuatrimestre;
        }

        return codigo.Cuatrimestre switch
        {
            1 or 2 => 1,
            3 or 4 => 2,
            5 or 6 => 3,
            _ => null,
        };
    }

    /// <summary>CUATRIMESTRE (solo terciarias): primer dígito del CUTUCO. null si no se puede decodificar.</summary>
    public static int? Cuatrimestre(int cutuco) =>
        CodigoComision.TryDescomponer(cutuco, out var codigo) ? codigo.Cuatrimestre : null;

    /// <summary>DIVISION: letra de la comisión (3º dígito del CUTUCO, 1=A … 6=F). null si no se puede decodificar.</summary>
    public static string? Division(int cutuco) =>
        CodigoComision.TryDescomponer(cutuco, out var codigo) && codigo.Comision is >= 1 and <= 6
            ? CodigoComision.ComisionEnLetras(codigo.Comision)
            : null;

    /// <summary>CONDICION (CR): 'RC' recursante si CURSADA.CONDICION es RECURSANDO; si no, 'R' regular.</summary>
    public static string Condicion(string? condicionCursada) =>
        string.Equals(condicionCursada?.Trim(), "RECURSANDO", StringComparison.OrdinalIgnoreCase) ? "RC" : "R";

    /// <summary>GENERO: 'MUJER' si SEXO es 'F'; cualquier otro valor (incluido nulo) es 'VARON', como el legacy.</summary>
    public static string Genero(string? sexo) =>
        string.Equals(sexo?.Trim(), "F", StringComparison.OrdinalIgnoreCase) ? "MUJER" : "VARON";

    /// <summary>TIPDOC: los 3 primeros caracteres de COD_ALU (DNI, CI, LE, PAS), sin relleno.</summary>
    public static string TipoDocumento(string? codigoAlumno) =>
        Normalizar(codigoAlumno)[..LongitudTipoDocumento].Trim();

    /// <summary>
    /// NRO. DOCUMENTO con puntos a partir de las posiciones fijas de COD_ALU (4-5 . 6-8 . 9-11),
    /// tal como lo informaba el legacy: "12.345.678" (los ceros a la izquierda se conservan).
    /// </summary>
    public static string NumeroDocumentoConPuntos(string? codigoAlumno)
    {
        var codigo = Normalizar(codigoAlumno);
        return $"{codigo[3..5]}.{codigo[5..8]}.{codigo[8..11]}";
    }

    /// <summary>DOCUM de la nómina impresa: tipo + espacio + número con puntos ("DNI 12.345.678").</summary>
    public static string DocumentoParaNomina(string? codigoAlumno) =>
        $"{TipoDocumento(codigoAlumno)} {NumeroDocumentoConPuntos(codigoAlumno)}";

    /// <summary>TITULO CON EL QUE INGRESA: colegio secundario + título (CSECU + TSECU). null si no hay datos.</summary>
    public static string? TituloIngreso(string? colegioSecundario, string? tituloSecundario)
    {
        var titulo = $"{colegioSecundario?.Trim()} {tituloSecundario?.Trim()}".Trim();
        return titulo.Length == 0 ? null : titulo;
    }

    /// <summary>Edad en años cumplidos a la fecha dada. null sin fecha de nacimiento o si es futura.</summary>
    public static int? Edad(DateOnly? fechaNacimiento, DateOnly hoy)
    {
        if (fechaNacimiento is null || fechaNacimiento.Value > hoy)
        {
            return null;
        }

        var nacimiento = fechaNacimiento.Value;
        var edad = hoy.Year - nacimiento.Year;
        if (hoy < nacimiento.AddYears(edad))
        {
            edad--;
        }

        return edad;
    }

    /// <summary>NOMHOJA: nombre de la hoja Excel por comisión ("CARRE-CUTUCO").</summary>
    public static string NombreHoja(string? codigoCarrera, int cutuco) =>
        $"{codigoCarrera?.Trim()}-{cutuco.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

    /// <summary>Lleva el código a los 11 caracteres físicos de COD_ALU para que las posiciones fijas no fallen con datos cortos.</summary>
    private static string Normalizar(string? codigoAlumno) =>
        (codigoAlumno ?? string.Empty).TrimEnd().PadRight(LongitudCodigoAlumno);
}
