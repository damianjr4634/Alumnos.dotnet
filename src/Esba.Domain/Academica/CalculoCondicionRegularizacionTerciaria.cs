namespace Esba.Domain.Academica;

/// <summary>
/// Notas del cursado y faltas de un alumno en una materia terciaria, más los flags
/// de la materia, necesarias para resolver la condición al regularizar. Las notas
/// vacías se representan con null (en el legacy un campo vacío valía 0); el valor
/// centinela <c>99</c> significa "parcial no rendido/ausente".
/// </summary>
public readonly record struct NotasRegularizacionTerciaria(
    string? CondicionActual,
    decimal? TpEva,
    decimal? TpEva2,
    decimal? Recup,
    int? TotHoras,
    int? Inasist,
    int? Justif,
    bool MateriaPromociona,
    bool MateriaApruebaSinFinal);

/// <summary>
/// Resuelve la condición de un alumno en una materia terciaria a partir de las notas
/// del cursado (2 parciales + recuperatorio) y las faltas. Porta el SP legacy
/// <c>XXX_REGULARIZACION_MAT_TERC</c> (que leía/escribía el staging <c>"$$$CURSADA"</c>)
/// a lógica de dominio pura, sin estado global (migration_improvements.md §1.2, §2.3).
/// </summary>
/// <remarks>
/// Lógica portada 1:1 del PSQL, incluidas sus particularidades: el centinela 99, la
/// precedencia del ajuste por faltas y la promoción/aprobación sin final por flags de
/// MATERIAS. Se cubre con tests unitarios y con equivalencia contra el SP.
/// </remarks>
public static class CalculoCondicionRegularizacionTerciaria
{
    private const string Cursando = "CURSANDO";
    private const string Recursa = "RECURSA";
    private const string Regular = "REGULAR";
    private const string Reincorpora = "REINCORPORA";
    private const string Libre = "LIBRE";
    private const string Promociona = "PROMOCIONA";
    private const string Final = "FINAL";

    /// <summary>
    /// Condición resultante y, si aprueba directo (PROMOCIONA/FINAL), la nota que pasa
    /// al analítico. La fecha de esa nota la resuelve la capa de datos (TBL_CUAT).
    /// </summary>
    public readonly record struct Resultado(string Condicion, decimal? NotaAnalitico)
    {
        /// <summary>true si la materia se aprueba directo y debe volcarse a ANALITIC.</summary>
        public bool VaAlAnalitico => NotaAnalitico is not null;
    }

    /// <summary>
    /// Resuelve condición + nota de analítico. Combina el SP de condición
    /// (<c>XXX_REGULARIZACION_MAT_TERC</c>) con el cálculo de la nota que hace la rama
    /// TER del commit (<c>XXX_REGULARIZACION</c>): PROMOCIONA → promedio de parciales;
    /// FINAL → recuperatorio si algún parcial fue 99/&lt;4, si no el promedio.
    /// </summary>
    public static Resultado Resolver(NotasRegularizacionTerciaria n, decimal notaPromocion)
    {
        var condicion = ResolverCondicion(n, notaPromocion);
        decimal? notaAnalitico = condicion switch
        {
            Promociona => Promedio(n),
            Final => (n.TpEva == 99m || n.TpEva2 == 99m || (n.TpEva ?? 0m) < 4m || (n.TpEva2 ?? 0m) < 4m)
                ? n.Recup
                : Promedio(n),
            _ => null,
        };
        return new Resultado(condicion, notaAnalitico);
    }

    private static decimal Promedio(NotasRegularizacionTerciaria n) => ((n.TpEva ?? 0m) + (n.TpEva2 ?? 0m)) / 2m;

    /// <param name="notaPromocion">Umbral <c>Regula_NotPromocion</c> de XXX_CONF.</param>
    public static string ResolverCondicion(NotasRegularizacionTerciaria n, decimal notaPromocion)
    {
        var condicionActual = (n.CondicionActual ?? string.Empty).Trim();

        // El legacy: si viene REINCORPORA pero aún no aprobó, el fallback es CURSANDO
        // (por si después le quitan las faltas y todavía le faltan notas).
        var condicionFallback = condicionActual == Reincorpora ? Cursando : condicionActual;

        var tpEva = n.TpEva ?? 0m;
        var tpEva2 = n.TpEva2 ?? 0m;
        var recup = n.Recup ?? 0m;

        var condicion = condicionActual;

        // Bloque A (IF independiente): sin ninguna nota, se mantiene la condición actual.
        if (tpEva == 0m && tpEva2 == 0m)
        {
            condicion = condicionFallback;
        }

        // Bloque B (cadena IF/ELSE IF): resuelve por notas.
        var algunoAusente = tpEva == 99m || tpEva2 == 99m;
        var algunoDesaprobado = EntreInclusive(tpEva, 0.1m, 3.99m) || EntreInclusive(tpEva2, 0.1m, 3.99m);

        if (tpEva != 0m && tpEva2 == 0m)
        {
            condicion = condicionFallback;                       // un solo parcial cargado
        }
        else if (algunoAusente && recup == 0m)
        {
            condicion = condicionFallback;
        }
        else if (algunoAusente && recup == 99m)
        {
            condicion = Recursa;
        }
        else if (algunoAusente && EntreInclusive(recup, 4m, 10m))
        {
            condicion = Regular;
        }
        else if (algunoAusente && recup < 4m)
        {
            condicion = Recursa;
        }
        else if (algunoDesaprobado && recup == 0m)
        {
            condicion = condicionFallback;
        }
        else if (algunoDesaprobado && recup == 99m)
        {
            condicion = Recursa;
        }
        else if (algunoDesaprobado && recup < 4m)
        {
            condicion = Recursa;
        }
        else if (algunoDesaprobado && recup >= 4m)
        {
            condicion = Regular;
        }
        else if (EntreInclusive(tpEva, 4m, 10m) && EntreInclusive(tpEva2, 4m, 10m))
        {
            condicion = Regular;
        }

        // Ajuste por faltas (solo si hay carga horaria; si TOT_HORAS = 0, se mantiene).
        var totHoras = n.TotHoras ?? 0;
        if (totHoras > 0)
        {
            var resulJustif = (decimal)Math.Round((n.Justif ?? 0) * 100m / totHoras, MidpointRounding.AwayFromZero);
            var resulInasist = (decimal)Math.Round((n.Inasist ?? 0) * 100m / totHoras, MidpointRounding.AwayFromZero);
            var resul = resulJustif + resulInasist;

            if (resul <= 25m && condicion is not (Recursa or "RECURSANDO" or Cursando))
            {
                condicion = Regular;
            }
            else if (resul is >= 26m and <= 50m && condicion != Recursa)
            {
                if (resulInasist <= 25m && condicion != Cursando)
                {
                    condicion = Regular;
                }
                else if (resulInasist > 25m)
                {
                    condicion = Reincorpora;
                }
            }
            else if (resul is > 40m and <= 60m && condicion != Recursa)
            {
                condicion = resulInasist is >= 25m and <= 50m
                    ? Reincorpora
                    : resulInasist > 50m ? Libre : Reincorpora;
            }
            else if (resul > 60m)
            {
                condicion = Libre;
            }
        }
        else
        {
            condicion = condicionFallback;
        }

        // Promoción / aprobación sin final: solo aplica sobre una materia REGULAR.
        if (condicion == Regular)
        {
            if (n.MateriaPromociona
                && tpEva != 99m && tpEva >= notaPromocion
                && tpEva2 != 99m && tpEva2 >= notaPromocion)
            {
                condicion = Promociona;
            }

            if (n.MateriaApruebaSinFinal && condicion == Regular)
            {
                condicion = Final;
            }
        }

        return condicion;
    }

    private static bool EntreInclusive(decimal valor, decimal min, decimal max) =>
        valor >= min && valor <= max;
}
