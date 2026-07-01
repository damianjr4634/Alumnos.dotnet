namespace Esba.Domain.Examenes;

/// <summary>
/// Descompone el código <c>CUTUCO</c> (3 dígitos) en sus tres componentes:
/// <b>CU</b>atrimestre, <b>TU</b>rno y <b>CO</b>misión, y los formatea para el
/// encabezado de las actas de examen. Sucesor del bloque que las pantallas legacy de
/// actas repetían: <c>Copy(IntToStr(CUTUCO),1,1)</c> (cuatrimestre), <c>Turnos(...)</c>
/// (2º dígito) y <c>Division(...)</c> (3º dígito) — ver lstactasARegular.pas /
/// lstactasexamenes.pas / lstactasMesas.pas.
/// </summary>
/// <remarks>
/// Mapeo confirmado por el usuario (2026-06-30): cuatrimestre 1–6; turno 1 mañana,
/// 2 tarde, 3 vespertino, 4 noche; comisión 1–6 interpretada como letra (1=A … 6=F).
/// </remarks>
public readonly record struct CodigoComision(int Cuatrimestre, int Turno, int Comision)
{
    /// <summary>
    /// Descompone un CUTUCO de tres dígitos (el legacy solo decodifica cuando
    /// <c>CUTUCO &gt;= 100</c>; los valores con menos dígitos no son descomponibles).
    /// </summary>
    public static bool TryDescomponer(int cutuco, out CodigoComision codigo)
    {
        codigo = default;
        if (cutuco < 100)
        {
            return false;
        }

        var texto = cutuco.ToString(System.Globalization.CultureInfo.InvariantCulture);
        codigo = new CodigoComision(
            Cuatrimestre: texto[0] - '0',
            Turno: texto[1] - '0',
            Comision: texto[2] - '0');
        return true;
    }

    /// <summary>Texto del turno (2º dígito del CUTUCO).</summary>
    public static string TurnoEnLetras(int turno) => turno switch
    {
        1 => "Mañana",
        2 => "Tarde",
        3 => "Vespertino",
        4 => "Noche",
        _ => turno.ToString(System.Globalization.CultureInfo.InvariantCulture),
    };

    /// <summary>
    /// Letra de la comisión (3º dígito del CUTUCO): 1=A, 2=B, … Sucesor de
    /// <c>Division(n)</c> del legacy (que rotulaba este dígito como "División").
    /// </summary>
    public static string ComisionEnLetras(int comision) =>
        comision is >= 1 and <= 26
            ? ((char)('A' + comision - 1)).ToString()
            : comision.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Texto del turno de esta instancia.</summary>
    public string TurnoTexto => TurnoEnLetras(Turno);

    /// <summary>Letra de la comisión de esta instancia.</summary>
    public string ComisionTexto => ComisionEnLetras(Comision);
}
