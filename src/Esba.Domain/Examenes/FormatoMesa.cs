using System.Globalization;

namespace Esba.Domain.Examenes;

/// <summary>
/// Formatos de presentación de una mesa de examen que las impresiones legacy armaban
/// en SQL (Impresiones.pas: Imp_Mesas_citacion / Imp_Mesas_ParteDiario). Funciones
/// puras con tests.
/// </summary>
public static class FormatoMesa
{
    /// <summary>
    /// MESAS.HORA NUMERIC(4,0) codifica HHMM (930 = 9:30, 1830 = 18:30). El legacy la
    /// partía con SUBSTRING sobre el número convertido a texto; acá se separan horas y
    /// minutos numéricamente. null o cero → vacío (el legacy producía basura con "0").
    /// </summary>
    public static string Hora(int? hora)
    {
        if (hora is null or <= 0)
        {
            return string.Empty;
        }

        var horas = hora.Value / 100;
        var minutos = hora.Value % 100;
        return string.Create(CultureInfo.InvariantCulture, $"{horas}:{minutos:00}");
    }

    /// <summary>
    /// Columna COMISION del parte diario: "COMI1/COMI2/COMI3" con 0 para los nulos. El
    /// SQL legacy repetía COMI3 en lugar de COMI2 (descuido corregido acá).
    /// </summary>
    public static string Comisiones(int? comision1, int? comision2, int? comision3) =>
        string.Create(CultureInfo.InvariantCulture, $"{comision1 ?? 0}/{comision2 ?? 0}/{comision3 ?? 0}");

    /// <summary>
    /// Columna PROFESORES del parte diario: titular, vocal 1 y vocal 2 separados por
    /// guion. El legacy truncaba cada nombre a 15 caracteres por el ancho fijo de la
    /// hoja (layout, no negocio) y dejaba guiones sueltos para los ausentes; acá se
    /// unen solo los presentes.
    /// </summary>
    public static string Docentes(string? titular, string? vocal1, string? vocal2) =>
        string.Join(" - ", new[] { titular, vocal1, vocal2 }
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d!.Trim()));
}
