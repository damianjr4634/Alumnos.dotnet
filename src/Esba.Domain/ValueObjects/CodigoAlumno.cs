using Esba.Domain.Common;

namespace Esba.Domain.ValueObjects;

/// <summary>
/// ALUMNOS.COD_ALU CHAR(11): código compuesto por el tipo de documento en
/// 3 caracteres ("DNI", "CI ", "LE ", "PAS") más el número en 8 dígitos con
/// ceros a la izquierda. Replica el armado de FrmAltaAlumno (zero-padding del
/// número y concatenación del tipo).
/// </summary>
public sealed record CodigoAlumno
{
    public const int LongitudTotal = 11;
    private const int LongitudTipo = 3;
    private const int LongitudNumero = 8;

    private static readonly string[] TiposValidos = ["DNI", "CI", "LE", "PAS"];

    private CodigoAlumno(string tipoDocumento, string numero)
    {
        TipoDocumento = tipoDocumento;
        Numero = numero;
    }

    /// <summary>Tipo de documento sin relleno: DNI, CI, LE o PAS.</summary>
    public string TipoDocumento { get; }

    /// <summary>Número de documento en 8 dígitos con ceros a la izquierda.</summary>
    public string Numero { get; }

    /// <summary>Representación física tal como se persiste en COD_ALU (11 caracteres).</summary>
    public string Valor => TipoDocumento.PadRight(LongitudTipo) + Numero;

    public static CodigoAlumno Crear(string tipoDocumento, string numeroDocumento)
    {
        ArgumentNullException.ThrowIfNull(tipoDocumento);
        ArgumentNullException.ThrowIfNull(numeroDocumento);

        var tipo = tipoDocumento.Trim().ToUpperInvariant();
        if (!TiposValidos.Contains(tipo))
        {
            throw new DomainException($"Tipo de documento inválido: '{tipoDocumento}'. Valores permitidos: DNI, CI, LE, PAS.");
        }

        var numero = numeroDocumento.Trim();
        if (numero.Length == 0 || numero.Length > LongitudNumero || !numero.All(char.IsAsciiDigit))
        {
            throw new DomainException($"Número de documento inválido: '{numeroDocumento}'. Debe ser numérico de hasta {LongitudNumero} dígitos.");
        }

        return new CodigoAlumno(tipo, numero.PadLeft(LongitudNumero, '0'));
    }

    /// <summary>Interpreta un COD_ALU leído de la base.</summary>
    public static CodigoAlumno Parse(string codAlu)
    {
        ArgumentNullException.ThrowIfNull(codAlu);
        if (codAlu.TrimEnd().Length != LongitudTotal)
        {
            throw new DomainException($"COD_ALU inválido: '{codAlu}'. Se esperan {LongitudTotal} caracteres (3 de tipo + 8 de número).");
        }

        return Crear(codAlu[..LongitudTipo], codAlu[LongitudTipo..]);
    }

    public override string ToString() => Valor;
}
