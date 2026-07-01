namespace Esba.Domain.Academica;

/// <summary>
/// Notas del cursado y faltas de un alumno en una materia de <b>bachillerato</b>
/// (la carrera 'BAC', única para la que el legacy corre el ladder de notas _POSTVAL),
/// más el flag de recursado, necesarias para resolver la condición al regularizar.
/// Las notas vacías se representan
/// con null (en el legacy un campo vacío valía 0); el centinela <c>99</c> significa
/// "no rendido/ausente".
/// </summary>
/// <param name="Paso">
/// Decisión del operador cuando las faltas dejan al alumno en CONSEJO (26-40% de
/// inasistencias): null/"" pide la decisión; "Consejo"/"Regular"/"Libre" la resuelven.
/// Porta el parámetro PASO + la salida FBUTTONS de <c>XXX_REGULARIZACION_MAT_POSTVAL</c>.
/// </param>
public readonly record struct NotasRegularizacionBachiller(
    string? CondicionActual,
    decimal? TpEva,
    decimal? TpEva2,
    decimal? Recup,
    decimal? Regular,
    int? TotHoras,
    int? Inasist,
    bool EnRecursa,
    string? Paso);

/// <summary>
/// Resuelve la condición de un alumno en una materia de bachillerato a partir de las
/// faltas y las notas del cursado (2 bimestres + recuperatorio + nota "a regular").
/// Porta a lógica de dominio pura los SP legacy <c>XXX_REGULARIZACION_MAT_BAC</c>
/// (ajuste por faltas) y <c>XXX_REGULARIZACION_MAT_POSTVAL</c> (validación por notas,
/// interactiva ante CONSEJO), que leían/escribían el staging <c>"$$$CURSADA"</c>. Sin
/// estado global (migration_improvements.md §1.2, §2.3).
/// </summary>
/// <remarks>
/// Flujo del legacy fusionado: primero <c>_BAC</c> clasifica por porcentaje de
/// inasistencias (≤25% sigue, 26-40% CONSEJO, &gt;40% LIBRES) y aplica el rescate por
/// tabla RECURSA; luego <c>_POSTVAL</c> resuelve por notas. CONSEJO es interactivo: sin
/// <see cref="NotasRegularizacionBachiller.Paso"/> el resultado pide la decisión
/// (Consejo/Regular/Libre). El promedio del cuatrimestre (TP_EVA3) y la nota definitiva
/// (FINAL1) —que el formulario legacy calculaba en la UI— se computan acá. Portado 1:1,
/// con equivalencia contra los SP.
/// </remarks>
public static class CalculoCondicionRegularizacionBachiller
{
    private const string Cursando = "CURSANDO";
    private const string Recursando = "RECURSANDO";
    private const string Consejo = "CONSEJO";
    private const string Libres = "LIBRES";
    private const string Regular = "REGULAR";
    private const string ARegular = "A/REGULAR";
    private const string Previo = "PREVIO";

    // CONTINASBAC internos (clasificación por faltas de _BAC / _POSTVAL).
    private const string ContinasRegular = "REGULAR";

    /// <summary>Opciones que ofrece _POSTVAL cuando el alumno queda en CONSEJO (FBUTTONS).</summary>
    public static readonly IReadOnlyList<string> OpcionesConsejo = ["Consejo", "Regular", "Libre"];

    /// <summary>
    /// Condición resultante y datos derivados. Cuando las faltas dejan al alumno en
    /// CONSEJO y no se pasó <c>Paso</c>, <see cref="RequiereDecision"/> es true y
    /// <see cref="Condicion"/> es null: la UI debe pedir la decisión (Consejo/Regular/Libre).
    /// </summary>
    public readonly record struct Resultado(
        string? Condicion,
        decimal? NotaFinal,
        decimal Promedio,
        bool RequiereDecision)
    {
        /// <summary>Opciones a ofrecer al operador cuando <see cref="RequiereDecision"/>.</summary>
        public IReadOnlyList<string> Opciones => RequiereDecision ? OpcionesConsejo : [];

        /// <summary>
        /// true si la materia se aprueba (condición REGULAR) y debe volcarse a ANALITIC
        /// con la nota <see cref="NotaFinal"/>. En bachillerato solo REGULAR va al analítico.
        /// </summary>
        public bool VaAlAnalitico => string.Equals(Condicion, Regular, StringComparison.Ordinal);
    }

    /// <summary>
    /// Resuelve la condición (y la nota definitiva / promedio) de una materia de bachillerato.
    /// </summary>
    public static Resultado Resolver(NotasRegularizacionBachiller n)
    {
        var paso = (n.Paso ?? string.Empty).Trim();
        var condiorg = (n.CondicionActual ?? string.Empty).Trim();

        var tpEva = n.TpEva ?? 0m;
        var tpEva2 = n.TpEva2 ?? 0m;
        var recup = n.Recup ?? 0m;
        var regular = n.Regular ?? 0m;
        var totHoras = n.TotHoras ?? 0;
        var inasist = n.Inasist ?? 0;

        // Derivados que el formulario legacy calculaba en la UI y volcaba al staging.
        var promedio = Promedio(tpEva, tpEva2);                              // TP_EVA3
        decimal? notaDefinitiva = recup != 0m ? (promedio + recup) / 2m : null; // FINAL1

        // Fase A — faltas (_BAC): determina CURSANDO/RECURSANDO/CONSEJO/LIBRES.
        var condicionBac = ClasificarPorFaltas(condiorg, totHoras, inasist, n.EnRecursa);

        // Fase B — notas (_POSTVAL).
        if (condicionBac == Libres)
        {
            // _POSTVAL no toca las materias que _BAC dejó libres por faltas.
            return new Resultado(Libres, null, promedio, RequiereDecision: false);
        }

        if (condicionBac == Consejo)
        {
            if (paso.Length == 0)
            {
                return new Resultado(Condicion: null, null, promedio, RequiereDecision: true);
            }

            return paso switch
            {
                "Consejo" => new Resultado(Consejo, null, promedio, false),
                "Libre" => new Resultado(Libres, null, promedio, false),
                "Regular" => Empaquetar(LadderDesdeConsejoRegular(tpEva, tpEva2, promedio, recup, regular, notaDefinitiva), promedio),
                _ => new Resultado(Consejo, null, promedio, false),
            };
        }

        // condicionBac ∈ {CURSANDO, RECURSANDO}: _POSTVAL recomputa CONTINASBAC y aplica el ladder.
        var continasbac = ContinasbacDesdeAsistencia(condicionBac, totHoras, inasist);
        var ladder = LadderInicial(continasbac, condicionBac, tpEva, tpEva2, promedio, recup, regular, notaDefinitiva);
        return Empaquetar(ladder, promedio);
    }

    private static Resultado Empaquetar((string Condicion, decimal? NotaFinal) ladder, decimal promedio) =>
        new(ladder.Condicion, ladder.NotaFinal, promedio, RequiereDecision: false);

    /// <summary>Promedio del cuatrimestre (TP_EVA3): 99 (ausente) cuenta como 1, como en el legacy.</summary>
    private static decimal Promedio(decimal tpEva, decimal tpEva2)
    {
        if (tpEva == 99m || tpEva2 == 99m)
        {
            var n1 = tpEva == 99m ? 1m : tpEva;
            var n2 = tpEva2 == 99m ? 1m : tpEva2;
            return (n1 + n2) / 2m;
        }

        return (tpEva + tpEva2) / 2m;
    }

    /// <summary>Clasificación por faltas de <c>XXX_REGULARIZACION_MAT_BAC</c>.</summary>
    private static string ClasificarPorFaltas(string condiorg, int totHoras, int inasist, bool enRecursa)
    {
        string condicion;
        if (totHoras == 0)
        {
            condicion = condiorg == Recursando ? Recursando : Cursando;
        }
        else
        {
            var resul = Math.Round(inasist * 100m / totHoras, MidpointRounding.AwayFromZero);
            condicion = resul switch
            {
                <= 25m => condiorg == Recursando ? Recursando : Cursando, // CONTINASBAC = REGULAR
                <= 40m => Consejo,
                _ => Libres,
            };
        }

        // Rescate por RECURSA: una cursada que quedó CURSANDO viniendo de otra condición
        // y que figura en RECURSA se marca RECURSANDO.
        if (condicion == Cursando && condiorg != Cursando && enRecursa)
        {
            condicion = Recursando;
        }

        return condicion;
    }

    /// <summary>CONTINASBAC que _POSTVAL recomputa de la asistencia (REGULAR / CONSEJO / LIBRE / la condición si no hay horas).</summary>
    private static string ContinasbacDesdeAsistencia(string condicionBac, int totHoras, int inasist)
    {
        if (totHoras == 0)
        {
            return condicionBac; // CURSANDO / RECURSANDO
        }

        var resul = Math.Round(inasist * 100m / totHoras, MidpointRounding.AwayFromZero);
        return resul switch
        {
            <= 25m => ContinasRegular,
            <= 40m => Consejo,
            _ => "LIBRE",
        };
    }

    // Ladder de notas de _POSTVAL, primera pasada (PASO=''). CONTINASBAC vale REGULAR
    // (asistencia ok) o CURSANDO/RECURSANDO (sin carga horaria): en el segundo caso el
    // resultado se mantiene en la condición de origen. Portado 1:1 del PSQL.
    private static (string Condicion, decimal? NotaFinal) LadderInicial(
        string continasbac, string condiorg,
        decimal tpEva, decimal tpEva2, decimal tpEva3, decimal recup, decimal regular, decimal? final1)
    {
        var reg = continasbac == ContinasRegular;
        var cur = continasbac == Cursando;

        if (tpEva == 99m && tpEva2 == 99m)
        {
            return (Libres, null);
        }

        if (tpEva == 0m || tpEva2 == 0m)
        {
            return (condiorg, null);
        }

        if (Entre(tpEva, 6m, 10m) && Entre(tpEva2, 6m, 10m))
        {
            if (reg)
            {
                return (Regular, tpEva3);
            }

            return (condiorg, null);
        }

        if (Entre(tpEva2, 4m, 10m) && tpEva3 >= 6m)
        {
            if (reg)
            {
                return (Regular, tpEva3);
            }

            return (condiorg, null);
        }

        if (Entre(tpEva3, 1m, 3.99m))
        {
            if (Entre(recup, 6m, 10m))
            {
                return reg ? (Regular, final1) : (condiorg, (decimal?)null);
            }

            if (regular == 0m)
            {
                return reg ? (ARegular, (decimal?)null) : (condiorg, (decimal?)null);
            }

            if (Entre(regular, 6m, 10m))
            {
                return reg ? (Regular, regular) : (condiorg, (decimal?)null);
            }

            if (Entre(regular, 1m, 5.99m) || regular == 99m)
            {
                return reg ? (Previo, (decimal?)null) : (condiorg, (decimal?)null);
            }

            return (condiorg, null);
        }

        if (Entre(tpEva, 6m, 10m) && Entre(tpEva2, 1m, 3.99m))
        {
            var condicion = condiorg;
            decimal? notaFinal = null;

            if (recup == 0m)
            {
                if (reg)
                {
                    condicion = Cursando; // el legacy fuerza CURSANDO acá (no condiorg)
                }
            }
            else if (Entre(recup, 6m, 10m))
            {
                if (reg)
                {
                    (condicion, notaFinal) = (Regular, final1);
                }
            }

            if (Entre(recup, 0.1m, 5.99m) || recup == 99m)
            {
                if (regular == 0m)
                {
                    if (reg)
                    {
                        condicion = ARegular;
                    }
                }
                else if (Entre(regular, 6m, 10m))
                {
                    if (reg)
                    {
                        (condicion, notaFinal) = (Regular, regular);
                    }
                }

                if (Entre(regular, 0.1m, 5.99m) || regular == 99m)
                {
                    if (reg)
                    {
                        condicion = Previo;
                    }
                }
            }

            return (condicion, notaFinal);
        }

        if (tpEva3 >= 4m && tpEva3 < 6m)
        {
            if (recup == 0m)
            {
                return (condiorg, null); // reg o cur: se mantiene la condición de origen
            }

            if (Entre(recup, 6m, 10m))
            {
                return reg ? (Regular, final1) : (condiorg, (decimal?)null);
            }

            if (Entre(recup, 1m, 5.99m) || recup == 99m)
            {
                var condicion = condiorg;
                decimal? notaFinal = null;

                if (regular == 0m)
                {
                    if (reg)
                    {
                        condicion = ARegular;
                    }
                }

                if (Entre(regular, 6m, 10m))
                {
                    if (reg)
                    {
                        (condicion, notaFinal) = (Regular, regular);
                    }
                }

                if (Entre(regular, 1m, 5.99m) || regular == 99m)
                {
                    if (reg)
                    {
                        condicion = Previo;
                    }
                }

                return (condicion, notaFinal);
            }
        }

        _ = cur; // el caso "CURSANDO" ya se resuelve manteniendo condiorg en cada rama.
        return (condiorg, null);
    }

    // Ladder de _POSTVAL cuando, desde CONSEJO, el operador elige "Regular": CONTINASBAC
    // se fuerza a REGULAR y los desenlaces que en la pasada inicial "seguían cursando"
    // pasan a CONSEJO. Portado 1:1 del bloque PASO='Regular' del PSQL.
    private static (string Condicion, decimal? NotaFinal) LadderDesdeConsejoRegular(
        decimal tpEva, decimal tpEva2, decimal tpEva3, decimal recup, decimal regular, decimal? final1)
    {
        if (tpEva == 99m && tpEva2 == 99m)
        {
            return (Libres, null);
        }

        if (tpEva == 0m || tpEva2 == 0m)
        {
            return (Consejo, null);
        }

        if (Entre(tpEva, 6m, 10m) && Entre(tpEva2, 6m, 10m))
        {
            return (Regular, tpEva3);
        }

        if (Entre(tpEva2, 4m, 10m) && tpEva3 >= 6m)
        {
            return (Regular, tpEva3);
        }

        if (Entre(tpEva3, 1m, 3.99m))
        {
            if (Entre(recup, 6m, 10m))
            {
                return (Regular, final1);
            }

            if (regular == 0m)
            {
                return (ARegular, null);
            }

            if (Entre(regular, 6m, 10m))
            {
                return (Regular, regular);
            }

            if (Entre(regular, 1m, 5.99m) || regular == 99m)
            {
                return (Previo, null);
            }

            return (Consejo, null);
        }

        if (Entre(tpEva, 6m, 10m) && Entre(tpEva2, 1m, 3.99m))
        {
            var condicion = Consejo;
            decimal? notaFinal = null;

            if (recup == 0m)
            {
                condicion = Consejo;
            }
            else if (Entre(recup, 6m, 10m))
            {
                (condicion, notaFinal) = (Regular, final1);
            }

            if (Entre(recup, 0.1m, 5.99m) || recup == 99m)
            {
                if (regular == 0m)
                {
                    condicion = ARegular;
                }
                else if (Entre(regular, 6m, 10m))
                {
                    (condicion, notaFinal) = (Regular, regular);
                }

                if (Entre(regular, 0.1m, 5.99m) || regular == 99m)
                {
                    condicion = Previo;
                }
            }

            return (condicion, notaFinal);
        }

        if (tpEva3 >= 4m && tpEva3 < 6m)
        {
            if (recup == 0m)
            {
                return (Consejo, null);
            }

            if (Entre(recup, 6m, 10m))
            {
                return (Regular, final1);
            }

            if (Entre(recup, 1m, 5.99m) || recup == 99m)
            {
                var condicion = Consejo;
                decimal? notaFinal = null;

                if (regular == 0m)
                {
                    condicion = ARegular;
                }

                if (Entre(regular, 6m, 10m))
                {
                    (condicion, notaFinal) = (Regular, regular);
                }

                if (Entre(regular, 1m, 5.99m) || regular == 99m)
                {
                    condicion = Previo;
                }

                return (condicion, notaFinal);
            }
        }

        return (Consejo, null);
    }

    private static bool Entre(decimal valor, decimal min, decimal max) => valor >= min && valor <= max;
}
