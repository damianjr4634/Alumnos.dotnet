using Esba.Domain.Common;
using Esba.Domain.ValueObjects;

namespace Esba.Domain.Tests.ValueObjects;

public class LibroMatrizTests
{
    [Fact]
    public void Parse_ValorDeLaBase_SeparaLibroYFolio()
    {
        // Misma partición que las columnas calculadas MATRIZ_L/MATRIZ_F.
        var matriz = LibroMatriz.Parse("01234");

        Assert.Equal("01", matriz.Libro);
        Assert.Equal("234", matriz.Folio);
        Assert.Equal("01234", matriz.Valor);
    }

    [Fact]
    public void Parse_ConRellenoDeChar_IgnoraEspaciosFinales()
    {
        var matriz = LibroMatriz.Parse("01234 ");

        Assert.Equal("01234", matriz.Valor);
    }

    [Fact]
    public void Parse_LongitudInvalida_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => LibroMatriz.Parse("123"));
    }
}
