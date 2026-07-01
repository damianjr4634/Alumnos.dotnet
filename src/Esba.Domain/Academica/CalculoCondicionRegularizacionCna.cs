namespace Esba.Domain.Academica;

/// <summary>
/// Resuelve la condición de un alumno en una materia de <b>CNA</b> al regularizar. A
/// diferencia de terciarias/bachillerato/secundario, CNA no usa ningún SP: la condición se
/// decide directamente por la <b>nota final</b> que carga el operador (lógica que el
/// formulario legacy resolvía en el cliente, <c>GrabaMateriaCNAClick</c>).
/// </summary>
/// <remarks>
/// Regla portada 1:1: nota ≥ 7 → REGULAR; nota ≥ 1 (y &lt; 7) → RECURSA; si no → CURSANDO.
/// La fecha del examen es obligatoria (el formulario legacy la exige antes de grabar); eso
/// se valida en la capa Application. El volcado usa la rama BAC del commit XXX_REGULARIZACION
/// (CNA es CARRERA.TIPO='BAC'): si queda REGULAR, la nota final va al analítico.
/// </remarks>
public static class CalculoCondicionRegularizacionCna
{
    private const string Regular = "REGULAR";
    private const string Recursa = "RECURSA";
    private const string Cursando = "CURSANDO";

    /// <summary>Condición resultante según la nota final (null/0 → CURSANDO).</summary>
    public static string Resolver(decimal? notaFinal)
    {
        var nota = notaFinal ?? 0m;
        if (nota >= 7m)
        {
            return Regular;
        }

        return nota >= 1m ? Recursa : Cursando;
    }

    /// <summary>true si la materia queda REGULAR y su nota final debe volcarse a ANALITIC.</summary>
    public static bool VaAlAnalitico(decimal? notaFinal) =>
        string.Equals(Resolver(notaFinal), Regular, StringComparison.Ordinal);
}
