using System.Globalization;

namespace Esba.Domain.Certificados;

/// <summary>
/// Composición de los textos de la resolución de equivalencia terciaria (formato nuevo de
/// lst_impresion_equivalencia_terc.pas): VISTO, CONSIDERANDO y los párrafos del Art. 1°.
/// Lógica pura, sin I/O.
/// </summary>
public static class ResolucionEquivalenciaFormatter
{
    public const string ArticuloPrimero = "Art 1° Dar aprobada/s por equivalencia/s las siguientes materias:";

    private const string ConsiderandoBase =
        "    La documentación presentada y las opiniones formuladas por el/los profesor/es de la/s " +
        "respectiva/s asignatura/s y en uso de las atribuciones que le confiere (Dispo. 272 / 23 y 24 " +
        "Art. 16, 17 y 18 de la Dirección General de Educación de Gestión Privada; según corresponda a " +
        "Institutos Técnicos o Institutos del Profesorado)";

    /// <summary>
    /// Normaliza la lista de cuatrimestres pedida (texto "2,3" o "2, 3") a enteros únicos
    /// y ordenados. Descarta tokens no numéricos.
    /// </summary>
    public static IReadOnlyList<int> ParsearCuatrimestres(string? entrada) =>
        (entrada ?? string.Empty)
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : (int?)null)
            .Where(n => n is not null)
            .Select(n => n!.Value)
            .Distinct()
            .OrderBy(n => n)
            .ToArray();

    public static string TextoVisto(
        string? nombreAlumno, string? codigoAlumno, int anio, IReadOnlyCollection<int> cuatrimestres, string? carrera)
    {
        var cuat = string.Join(",", cuatrimestres);
        return $"    La solicitud presentada por el/la alumno/a {(nombreAlumno ?? string.Empty).Trim()} " +
               $"{(codigoAlumno ?? string.Empty).Trim()} matriculado/a en año {anio} en el/los cuatrimestre/s " +
               $"{cuat} de la carrera {(carrera ?? string.Empty).Trim()} en el sentido de que se le por aprobada " +
               "por equivalencia la/s asignatura/s del plan de estudios de la citada carrera especificado en el " +
               "Artículo 1 de la presente Resolución.";
    }

    public static string TextoConsiderando(string? rector) =>
        $"{ConsiderandoBase} El/La Rector/a del Instituto {(rector ?? string.Empty).Trim()}";

    /// <summary>
    /// Párrafo de una materia para el Art. 1°. Corrige el faltante de espacio antes del
    /// establecimiento que tenía el legacy ("Establecimiento"+FEQINST).
    /// </summary>
    public static string ParrafoMateria(
        string? descripcion, int cuatrimestre, string? actaInterna,
        string? materiaOrigen, string? carreraOrigen, string? institutoOrigen, string? docente)
    {
        var ordinal = TextoCastellano.CuatrimestreEnLetras(cuatrimestre);
        return $"Materia {(descripcion ?? string.Empty).Trim()} del {ordinal} cuatrimestre con Acta Interna N° " +
               $"{(actaInterna ?? string.Empty).Trim()}. Habiendo cursado la materia {(materiaOrigen ?? string.Empty).Trim()} " +
               $"de la carrera {(carreraOrigen ?? string.Empty).Trim()} en el Establecimiento {(institutoOrigen ?? string.Empty).Trim()}. " +
               $"La misma fue evaluada por el docente {(docente ?? string.Empty).Trim()}";
    }
}
