namespace Esba.Domain.Academica;

/// <summary>
/// Variante de regularización de una carrera. Misma decisión que toma la pantalla de
/// regularización por comisión de secretaría (hito 15): el tipo TER manda a terciarias;
/// los códigos BAC, 333/650 y CNA tienen cada uno su régimen; el resto todavía no tiene
/// regularización en el sistema nuevo.
/// </summary>
public enum VarianteRegularizacion
{
    Terciaria,
    Bachillerato,
    Secundario,
    Cna,
    NoDisponible,
}

/// <summary>
/// Qué campos del cursado le pide la precarga al docente para una carrera: exactamente
/// los que secretaría edita en la regularización de esa variante (hito 15), con sus
/// etiquetas. Diciembre/marzo (333/650), fecha y "paso" (BAC) son decisiones de
/// secretaría y no se precargan.
/// </summary>
public sealed record EsquemaCargaCursado
{
    public required VarianteRegularizacion Variante { get; init; }

    public bool Evaluacion1 { get; init; }

    public bool Evaluacion2 { get; init; }

    public bool Evaluacion3 { get; init; }

    public bool Recuperatorio { get; init; }

    /// <summary>CURSADA.REGULAR: nota "a regularizar" del bachillerato.</summary>
    public bool NotaRegular { get; init; }

    /// <summary>CURSADA.FINAL1: nota final de CNA.</summary>
    public bool NotaFinal { get; init; }

    public bool Horas { get; init; }

    public bool Inasistencias { get; init; }

    public bool Justificadas { get; init; }

    public string EtiquetaEvaluacion1 { get; init; } = "1° Parc.";

    public string EtiquetaEvaluacion2 { get; init; } = "2° Parc.";

    public string EtiquetaEvaluacion3 { get; init; } = "3° Parc.";

    public bool Disponible => Variante != VarianteRegularizacion.NoDisponible;

    public static VarianteRegularizacion DeterminarVariante(string codigoCarrera, string? tipoCarrera)
    {
        ArgumentNullException.ThrowIfNull(codigoCarrera);
        var codigo = codigoCarrera.Trim().ToUpperInvariant();
        var tipo = tipoCarrera?.Trim().ToUpperInvariant();

        // Mismo orden que RegularizacionComision.razor: los códigos especiales primero.
        if (codigo == "BAC")
        {
            return VarianteRegularizacion.Bachillerato;
        }

        if (codigo is "333" or "650")
        {
            return VarianteRegularizacion.Secundario;
        }

        if (codigo == "CNA")
        {
            return VarianteRegularizacion.Cna;
        }

        return tipo == "TER" ? VarianteRegularizacion.Terciaria : VarianteRegularizacion.NoDisponible;
    }

    public static EsquemaCargaCursado Para(string codigoCarrera, string? tipoCarrera) =>
        Para(DeterminarVariante(codigoCarrera, tipoCarrera));

    public static EsquemaCargaCursado Para(VarianteRegularizacion variante) => variante switch
    {
        VarianteRegularizacion.Terciaria => new()
        {
            Variante = variante,
            Evaluacion1 = true, Evaluacion2 = true, Recuperatorio = true,
            Horas = true, Inasistencias = true, Justificadas = true,
        },
        VarianteRegularizacion.Bachillerato => new()
        {
            Variante = variante,
            Evaluacion1 = true, Evaluacion2 = true, Recuperatorio = true, NotaRegular = true,
            Horas = true, Inasistencias = true, Justificadas = true,
            EtiquetaEvaluacion1 = "1° Bim.", EtiquetaEvaluacion2 = "2° Bim.",
        },
        VarianteRegularizacion.Secundario => new()
        {
            Variante = variante,
            Evaluacion1 = true, Evaluacion2 = true, Evaluacion3 = true,
            Horas = true, Inasistencias = true,
            EtiquetaEvaluacion1 = "1° Trim.", EtiquetaEvaluacion2 = "2° Trim.", EtiquetaEvaluacion3 = "3° Trim.",
        },
        VarianteRegularizacion.Cna => new()
        {
            Variante = variante,
            NotaFinal = true,
        },
        _ => new() { Variante = VarianteRegularizacion.NoDisponible },
    };
}
