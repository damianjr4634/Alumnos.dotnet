namespace Esba.Domain.Academica;

/// <summary>
/// Notas del cursado de un alumno en una materia de <b>secundario</b> (carreras 333 y 650),
/// necesarias para resolver la condición al regularizar. El régimen es trimestral con
/// exámenes de diciembre y marzo; las notas vacías se representan con null (en el legacy
/// valían 0) y el centinela <c>99</c> significa "ausente/aplazado".
/// </summary>
/// <remarks>
/// La condición se decide por la nota del <b>segundo trimestre</b> (<paramref name="TpEva2"/>)
/// y, si no alcanza, por los exámenes de <b>diciembre</b> y <b>marzo</b>. El régimen 333 no
/// aplica ajuste por faltas ni el flujo interactivo CONSEJO del bachillerato.
/// </remarks>
public readonly record struct NotasRegularizacion333(
    string? CondicionActual,
    decimal? TpEva,
    decimal? TpEva2,
    decimal? NotaDic,
    decimal? NotaMar,
    DateTime? FecEva2,
    DateTime? FechDic,
    DateTime? FechMar);

/// <summary>
/// Resuelve la condición de un alumno en una materia de secundario (333/650) al regularizar.
/// Porta a lógica de dominio pura el SP legacy <c>XXX_REGULARIZACION_MAT_333</c> (que
/// leía/escribía el staging <c>"$$$CURSADA"</c>), sin estado global (§1.2, §2.3).
/// </summary>
/// <remarks>
/// Se porta 1:1 la lógica <b>activa</b> del SP (el bloque de PROM/DICIEMBRE/MARZO estaba
/// comentado y no corría): 2° trimestre ≥ 6 → REGULAR; si el 2° no alcanza (o ambos
/// trimestres ausentes), se evalúan diciembre y marzo → REGULAR / PREVIA / ENPROCESO;
/// si no, se mantiene la condición de origen. La nota al analítico (NOTAFIN) es la del 2°
/// trimestre, diciembre o marzo, con su fecha. Cubierto con tests unitarios y equivalencia.
/// </remarks>
public static class CalculoCondicionRegularizacion333
{
    private const string Regular = "REGULAR";
    private const string EnProceso = "ENPROCESO";
    private const string Previa = "PREVIA";

    /// <summary>
    /// Condición resultante, la nota que va al analítico (NOTAFIN) con su fecha, y el flag
    /// de error del legacy: si diciembre/marzo aprueban (≥6) pero falta su fecha, el SP
    /// devolvía FERRCOD=2 y no grababa; acá se marca <see cref="FaltaFecha"/>.
    /// </summary>
    public readonly record struct Resultado(
        string Condicion,
        decimal NotaFinal,
        DateTime? NotaFinalFecha,
        bool FaltaFecha)
    {
        /// <summary>true si la materia queda REGULAR y debe volcarse a ANALITIC.</summary>
        public bool VaAlAnalitico => string.Equals(Condicion, Regular, StringComparison.Ordinal);
    }

    public static Resultado Resolver(NotasRegularizacion333 n)
    {
        var condiorg = (n.CondicionActual ?? string.Empty).Trim();
        var tpEva = n.TpEva ?? 0m;
        var tpEva2 = n.TpEva2 ?? 0m;
        var notaDic = n.NotaDic ?? 0m;
        var notaMar = n.NotaMar ?? 0m;

        // 2° trimestre aprobado (≥6 y no ausente) → REGULAR con la nota del 2° trimestre.
        if (tpEva2 >= 6m && tpEva2 != 99m)
        {
            return new Resultado(Regular, tpEva2, n.FecEva2, FaltaFecha: false);
        }

        // Ambos trimestres ausentes, o 2° trimestre desaprobado (0 < nota < 6): a diciembre/marzo.
        if ((tpEva == 99m && tpEva2 == 99m) || (tpEva2 > 0m && tpEva2 < 6m))
        {
            // Nota: el legacy evalúa "NOTADIC >= 6" antes que la rama de aplazo, por lo que un
            // diciembre en 99 (99 >= 6) queda REGULAR con nota 99. Se replica el SP tal cual.
            if (notaDic >= 6m)
            {
                return new Resultado(Regular, notaDic, n.FechDic, FaltaFecha: n.FechDic is null);
            }

            if (notaMar >= 6m)
            {
                return new Resultado(Regular, notaMar, n.FechMar, FaltaFecha: n.FechMar is null);
            }

            if ((notaDic > 0m && notaDic < 6m) || (notaMar > 0m && notaMar < 6m) || notaDic == 99m || notaMar == 99m)
            {
                return new Resultado(Previa, 0m, null, FaltaFecha: false);
            }

            // Sin diciembre ni marzo cargados: sigue en proceso.
            return new Resultado(EnProceso, 0m, null, FaltaFecha: false);
        }

        // Resto (p. ej. 2° trimestre sin cargar): se mantiene la condición de origen.
        return new Resultado(condiorg, 0m, null, FaltaFecha: false);
    }
}
