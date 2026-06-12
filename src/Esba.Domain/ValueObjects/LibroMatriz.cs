using Esba.Domain.Common;

namespace Esba.Domain.ValueObjects;

/// <summary>
/// ALUMNOS.MATRIZ CHAR(5): número de libro matriz compuesto por libro (2
/// caracteres) y folio (3 caracteres), según las columnas calculadas legacy
/// MATRIZ_L = SUBSTRING(1,2) y MATRIZ_F = SUBSTRING(3,3). Una vez asignado no
/// puede blanquearse (trigger ALUMNOS_BU0).
/// </summary>
public sealed record LibroMatriz
{
    public const int LongitudTotal = 5;
    private const int LongitudLibro = 2;

    private LibroMatriz(string libro, string folio)
    {
        Libro = libro;
        Folio = folio;
    }

    public string Libro { get; }

    public string Folio { get; }

    /// <summary>Representación física tal como se persiste en MATRIZ (5 caracteres).</summary>
    public string Valor => Libro + Folio;

    public static LibroMatriz Parse(string matriz)
    {
        ArgumentNullException.ThrowIfNull(matriz);
        var valor = matriz.TrimEnd();
        if (valor.Length != LongitudTotal)
        {
            throw new DomainException($"Libro matriz inválido: '{matriz}'. Se esperan {LongitudTotal} caracteres (2 de libro + 3 de folio).");
        }

        return new LibroMatriz(valor[..LongitudLibro], valor[LongitudLibro..]);
    }

    public override string ToString() => Valor;
}
