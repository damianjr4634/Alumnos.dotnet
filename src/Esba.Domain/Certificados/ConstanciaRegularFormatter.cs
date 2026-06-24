namespace Esba.Domain.Certificados;

/// <summary>
/// Compone el cuerpo de la Constancia de Alumno Regular, sucesor del armado de
/// <c>texto</c> en <c>CreatePDF</c> de constanciaalumnoregular.pas (la salida PDF, que
/// es la que se guarda/envía). El texto se reproduce literal; las únicas variantes son
/// presencial vs a distancia y cuatrimestre vs año. Lógica pura y testeable (§2.1.3).
/// </summary>
public static class ConstanciaRegularFormatter
{
    public const string Titulo = "CONSTANCIA DE ALUMNO REGULAR";

    public const string NotaLegal =
        "NOTA: La presente certificación carecerá de valor si no estuviera firmada por la autoridad competente.-";

    /// <summary>Devuelve los párrafos del cuerpo, en orden, ya resueltos.</summary>
    public static IReadOnlyList<string> Cuerpo(ConstanciaRegularContexto ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);

        var nombre = ctx.NombreCompleto.Trim();
        var codigo = ctx.CodigoConPuntos.Trim();
        var carrera = ctx.NombreCarrera.Trim().ToUpperInvariant();
        var anteQuien = ctx.AnteQuien.Trim().ToUpperInvariant();
        var cuatLetra = TextoCastellano.CuatrimestreEnLetrasMinuscula(ctx.Cuatrimestre);

        var parrafos = new List<string>();

        if (ctx.EsADistancia)
        {
            parrafos.Add(
                $"      Por la presente certificamos que  {nombre} {codigo}, es alumno regular del {cuatLetra} " +
                $"cuatrimestre de la carrera {carrera} del Ministerio de Educación del Gobierno de la Ciudad " +
                "Autónoma de Buenos Aires y Dictamen del Consejo Federal de Educación, del Ministerio de " +
                $"Educación de la Nación N° {ctx.Dictamen?.Trim()}.");
        }
        else
        {
            var unidad = ctx.EsCarreraPorAnio ? "año" : "cuatrimestre";
            parrafos.Add(
                $"      Por la presente certificamos que  {nombre} {codigo}, es alumno regular de la Carrera " +
                $"{carrera} - {cuatLetra} {unidad}.");
            parrafos.Add($"Asiste los dias: {Horario(ctx.CodigoCarrera, ctx.Turno)}");
        }

        parrafos.Add($"Se extiende la presente certificación para ser presentada ante  {anteQuien}.");
        parrafos.Add(
            $"A los  {ctx.Fecha.Day} días del mes de {TextoCastellano.MesEnLetras(ctx.Fecha.Month)} de {ctx.Fecha.Year}.");

        return parrafos;
    }

    /// <summary>
    /// Horario de cursada según carrera y turno (segundo dígito del CUTUCO). Tabla
    /// hardcodeada en el legacy (FormShow de constanciaalumnoregular.pas).
    /// // TODO-migrar: pasar estos horarios a una tabla de configuración (hoy no existe
    /// en el esquema; el legacy los tenía fijos en el código).
    /// </summary>
    public static string Horario(string codigoCarrera, int turno)
    {
        var carrera = (codigoCarrera ?? string.Empty).Trim().ToUpperInvariant();

        if (carrera == "BAC")
        {
            return turno switch
            {
                1 => "Lunes a Viernes en el horario de 8:30 a 11:30 hs.",
                2 => "Lunes a Viernes en el horario de 13:30 a 16:30 hs.",
                3 => "Lunes a Viernes en el horario de 17:30 a 20:00 hs.",
                4 => "Lunes a Viernes en el horario de 19:00 a 22:00 hs.",
                _ => "Sin horario definido",
            };
        }

        if (carrera is "333" or "650")
        {
            return turno switch
            {
                1 => "Lunes a Viernes en el horario de 8:00 a 13:00 hs.",
                2 => "Lunes a Viernes en el horario de 13:00 a 18:00 hs.",
                _ => "Sin horario definido",
            };
        }

        return turno switch
        {
            1 => "Lunes a Viernes en el horario de 8:30 a 11:45 hs.",
            2 => "Lunes a Viernes en el horario de 13:30 a 16:45 hs.",
            3 => "Lunes a Viernes en el horario de 17:15 a 20:00 hs.",
            4 => "Lunes a Viernes en el horario de 19:00 a 22:00 hs.",
            _ => "Sin horario definido",
        };
    }

    /// <summary>
    /// Línea de subvención del Estado según el tipo de carrera (TER 80% / BAC 70%);
    /// null para otros tipos. // TODO-confirmar: el legacy se contradice sobre cuándo
    /// mostrarla (impresión: solo presencial; PDF: solo a distancia). Se decidió
    /// mostrarla siempre para TER/BAC por ser un dato de la carrera, no de la modalidad.
    /// </summary>
    public static string? LineaSubvencion(string? tipoCarrera) =>
        (tipoCarrera?.Trim().ToUpperInvariant()) switch
        {
            "TER" => "La mencionada carrera cuenta con el 80% de la subvención del Estado",
            "BAC" => "La mencionada carrera cuenta con el 70% de la subvención del Estado",
            _ => null,
        };
}
