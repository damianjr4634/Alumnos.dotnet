using System.Globalization;

namespace Esba.Domain.Certificados;

/// <summary>
/// Formatea las filas del reporte tabular "Constancia de Materias Aprobadas" (CMA),
/// replicando la lógica por fila del legacy <c>constanciaalumnos2.pas</c>
/// (<c>BitBtn1Click</c>): según la condición de la materia, decide si la fila lleva
/// las cuatro columnas (condición/nota/fecha/instituto) o un único texto que las
/// abarca (materia anual, aprobada por equivalencia o eximida).
/// </summary>
/// <remarks>
/// El legacy dibuja en un canvas GDI y usa corridas de guiones ("------",
/// "--------------") como marcador de "sin dato" y un adorno
/// "---------- M A T E R I A  A N U A L ----------". Acá se normaliza a un guion
/// largo ("—") y a "MATERIA ANUAL" (presentación más limpia en una tabla real); el
/// contenido informativo es idéntico.
/// </remarks>
public static class ConstanciaMateriasAprobadasFormatter
{
    private const string SinDato = "—";
    private const string TextoAnual = "MATERIA ANUAL";
    private const string Adeuda = "* ADEUDA *";

    public static IReadOnlyList<FilaAnaliticoConstancia> Formatear(IReadOnlyList<MateriaAnaliticoConstancia> materias)
    {
        ArgumentNullException.ThrowIfNull(materias);

        var filas = new List<FilaAnaliticoConstancia>(materias.Count);
        foreach (var materia in materias)
        {
            filas.Add(FormatearFila(materia));
        }

        return filas;
    }

    /// <summary>Formatea una sola fila (expuesto para testear cada rama por separado).</summary>
    public static FilaAnaliticoConstancia FormatearFila(MateriaAnaliticoConstancia materia)
    {
        ArgumentNullException.ThrowIfNull(materia);

        if (materia.EsAnual)
        {
            return FilaUnica(materia, TextoAnual);
        }

        var condicion = (materia.Condicion ?? string.Empty).Trim();
        if (condicion is "EQUIVALENCIA" or "EXIMIDO")
        {
            return FilaUnica(materia, TextoEquivalenciaOEximido(materia, condicion));
        }

        return new FilaAnaliticoConstancia
        {
            Cuatrimestre = materia.Cuatrimestre,
            Materia = materia.Descripcion,
            Condicion = condicion == Adeuda ? "ADEUDA" : condicion,
            Nota = (materia.Nota ?? 0m) != 0m
                ? materia.Nota!.Value.ToString("0.00", CultureInfo.InvariantCulture)
                : SinDato,
            Fecha = materia.Fecha is { } f ? f.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : SinDato,
            Instituto = string.IsNullOrWhiteSpace(materia.Instituto)
                ? SinDato
                : (materia.Instituto.Trim() + " " + (materia.Caracteristica ?? string.Empty).Trim()).Trim(),
        };
    }

    // EXIMIDO → la descripción de la eximición; si no, aprobada por equivalencia con
    // el N° de acta interna (ACTINT) o, en su defecto, de D.G.E.G.P. (ACTDEGP).
    private static string TextoEquivalenciaOEximido(MateriaAnaliticoConstancia materia, string condicion)
    {
        if (condicion == "EXIMIDO")
        {
            return (materia.EximidoDescripcion ?? string.Empty).Trim();
        }

        if (!string.IsNullOrWhiteSpace(materia.ActividadInterna))
        {
            return "APROBADA POR EQUIVALENCIA - Act. Interna N° " + materia.ActividadInterna.Trim();
        }

        if (!string.IsNullOrWhiteSpace(materia.ActividadDgegp))
        {
            return "APROBADA POR EQUIVALENCIA - Act. D.G.E.G.P. N° " + materia.ActividadDgegp.Trim();
        }

        return "APROBADA POR EQUIVALENCIA";
    }

    private static FilaAnaliticoConstancia FilaUnica(MateriaAnaliticoConstancia materia, string texto) => new()
    {
        Cuatrimestre = materia.Cuatrimestre,
        Materia = materia.Descripcion,
        Condicion = texto,
        OcupaFilaCompleta = true,
    };
}
