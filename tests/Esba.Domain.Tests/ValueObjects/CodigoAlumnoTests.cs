using Esba.Domain.Common;
using Esba.Domain.ValueObjects;

namespace Esba.Domain.Tests.ValueObjects;

public class CodigoAlumnoTests
{
    [Fact]
    public void Crear_DniConNumeroCorto_RellenaConCeros()
    {
        var codigo = CodigoAlumno.Crear("DNI", "1234567");

        Assert.Equal("01234567", codigo.Numero);
        Assert.Equal("DNI01234567", codigo.Valor);
    }

    [Fact]
    public void Crear_TipoDeDosLetras_RellenaElTipoATresCaracteres()
    {
        // Replica el armado legacy: 'CI ' + 8 dígitos = 11 caracteres.
        var codigo = CodigoAlumno.Crear("CI", "30123456");

        Assert.Equal("CI 30123456", codigo.Valor);
        Assert.Equal(CodigoAlumno.LongitudTotal, codigo.Valor.Length);
    }

    [Fact]
    public void Crear_TipoInvalido_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CodigoAlumno.Crear("XX", "12345678"));
    }

    [Fact]
    public void Crear_NumeroNoNumerico_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CodigoAlumno.Crear("DNI", "12A45678"));
    }

    [Fact]
    public void Parse_ValorDeLaBase_SeparaTipoYNumero()
    {
        var codigo = CodigoAlumno.Parse("DNI30123456");

        Assert.Equal("DNI", codigo.TipoDocumento);
        Assert.Equal("30123456", codigo.Numero);
    }

    [Fact]
    public void Parse_LongitudInvalida_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => CodigoAlumno.Parse("DNI123"));
    }
}
