namespace Esba.Domain.Examenes;

/// <summary>
/// Resultado del análisis de un final cargado: la condición nueva del alumno en
/// la materia y, si corresponde, la nota que pasa al analítico.
/// </summary>
/// <param name="Condicion">Condición resultante (REGULAR/LIBRE/FINAL/RECURSA/…).</param>
/// <param name="NotaAnalitico">Nota que se vuelca a ANALITIC, o null si el final no aprueba.</param>
/// <param name="FechaAnalitico">Fecha de esa nota.</param>
/// <param name="ActaAnalitico">Libro/acta (FACTFIN) de esa nota.</param>
public readonly record struct ResultadoFinal(
    string Condicion,
    decimal? NotaAnalitico,
    DateOnly? FechaAnalitico,
    string? ActaAnalitico)
{
    /// <summary>true si el final aprobado debe moverse a CURSADA_HST + ANALITIC.</summary>
    public bool AprobóAlAnalitico => NotaAnalitico is not null;
}

/// <summary>
/// Una nota de final cargada (nota + fecha + libro de acta). Las notas vacías
/// se representan con null (en el legacy, un TCurrencyEdit vacío valía 0).
/// </summary>
public readonly record struct NotaFinal(decimal? Nota, DateOnly? Fecha, string? Acta);

/// <summary>
/// Cálculo de la condición resultante de un examen final y de la nota que pasa
/// al analítico. Porta la lógica que el legacy tenía repartida en la UI
/// (FinalesxMesayComision.GraboNotaASC/GraboNotaBAC) y en el SP de volcado
/// (XXX_MESAS): la condición la decidía la UI y la nota del analítico el SP.
///
/// Lógica de dominio pura, sin dependencias (migration_improvements.md §1.2):
/// se valida con tests unitarios y con el test de equivalencia contra XXX_MESAS.
/// Las "condiciones" son texto libre histórico (no hay enum hasta sanear el
/// dominio, ver <see cref="Esba.Domain.Entities.Cursada"/>).
/// </summary>
public static class CalculoCondicionFinal
{
    private const string Recursa = "RECURSA";
    private const string Final = "FINAL";
    private const string Libre = "LIBRE";

    /// <summary>
    /// Terciaria (tipo 'TER'). Porta GraboNotaASC: hasta tres notas; si las tres
    /// son aplazos (en [1,4)) el alumno RECURSA; si alguna aprueba ([4,10]) queda
    /// en FINAL; si no, conserva su condición anterior. Si queda FINAL, la nota
    /// del analítico es la primera de las tres que aprueba (XXX_MESAS, rama TER).
    /// </summary>
    public static ResultadoFinal Terciaria(NotaFinal n1, NotaFinal n2, NotaFinal n3, string condicionAnterior)
    {
        ArgumentNullException.ThrowIfNull(condicionAnterior);

        var v1 = n1.Nota ?? 0m;
        var v2 = n2.Nota ?? 0m;
        var v3 = n3.Nota ?? 0m;

        string condicion;
        if (EsAplazo(v1) && EsAplazo(v2) && EsAplazo(v3))
        {
            condicion = Recursa;
        }
        else if (Aprueba(v1) || Aprueba(v2) || Aprueba(v3))
        {
            condicion = Final;
        }
        else
        {
            condicion = condicionAnterior;
        }

        // Nota al analítico: solo si quedó FINAL, la primera nota que aprueba.
        if (condicion == Final)
        {
            foreach (var n in new[] { n1, n2, n3 })
            {
                if (Aprueba(n.Nota ?? 0m))
                {
                    return new ResultadoFinal(condicion, n.Nota, n.Fecha, n.Acta);
                }
            }
        }

        return new ResultadoFinal(condicion, null, null, null);

        static bool EsAplazo(decimal nota) => nota > 0 && nota < 4;
        static bool Aprueba(decimal nota) => nota is >= 4 and <= 10;
    }

    /// <summary>
    /// Bachiller (tipos 'BAC'/'BAD'). Porta GraboNotaBAC: una sola nota; si aprueba
    /// ([6,10]) la condición resultante depende de la anterior (PREVIO/PREVIA,
    /// LIBRES, DICIEMBRE, MARZO, LIBRE → LIBRE; P/EQUIVALEN → FINAL); si no aprueba,
    /// conserva la anterior. Si la condición queda en REGULAR/LIBRE/FINAL y la nota
    /// está en [6,10], esa nota pasa al analítico (XXX_MESAS, rama BAC/BAD).
    /// </summary>
    public static ResultadoFinal Bachiller(NotaFinal n1, string condicionAnterior)
    {
        ArgumentNullException.ThrowIfNull(condicionAnterior);

        var nota = n1.Nota ?? 0m;
        var anterior = condicionAnterior.Trim();

        var condicion = anterior;
        if (nota is >= 6 and <= 10)
        {
            condicion = anterior.ToUpperInvariant() switch
            {
                "PREVIO" or "PREVIA" => Libre,
                "LIBRES" => Libre,
                "DICIEMBRE" => Libre,
                "MARZO" => Libre,
                "LIBRE" => Libre,
                "P/EQUIVALEN" => Final,
                // TODO-confirmar: NotasExamenFinal.pas además mapea 'FINAL'→'FINAL';
                // FinalesxMesayComision.pas (el flujo migrado) no lo contempla. Ante
                // una condición no listada se conserva la anterior, como el legacy.
                _ => anterior,
            };
        }

        // Nota al analítico: condición REGULAR/LIBRE/FINAL y nota aprobada [6,10].
        if ((condicion is "REGULAR" or "LIBRE" or "FINAL") && nota is >= 6 and <= 10)
        {
            return new ResultadoFinal(condicion, n1.Nota, n1.Fecha, n1.Acta);
        }

        return new ResultadoFinal(condicion, null, null, null);
    }
}
