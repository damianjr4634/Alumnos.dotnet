using Esba.Domain.Academica;
using Xunit;

namespace Esba.Domain.Tests.Academica;

public class BloqueHorarioTests
{
    [Theory]
    [InlineData(false, false, false, "BLANCO")]
    [InlineData(true, false, false, "PRIMERO")]
    [InlineData(false, true, false, "SEGUNDO")]
    [InlineData(false, false, true, "TERCERO")]
    [InlineData(true, true, false, "PRISEG")]
    [InlineData(true, false, true, "PRITER")]
    [InlineData(false, true, true, "SEGTER")]
    [InlineData(true, true, true, "UNICO")]
    public void Codificar_DevuelveElCodigoLegacy(bool primero, bool segundo, bool tercero, string esperado)
    {
        Assert.Equal(esperado, BloqueHorario.Codificar(primero, segundo, tercero));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void CodificarYDecodificar_EsRoundtrip(bool primero, bool segundo, bool tercero)
    {
        var codigo = BloqueHorario.Codificar(primero, segundo, tercero);

        var (p, s, t) = BloqueHorario.Decodificar(codigo);

        Assert.Equal((primero, segundo, tercero), (p, s, t));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("  ", true)]
    [InlineData("BLANCO", true)]
    [InlineData("blanco", true)]
    [InlineData("UNICO", false)]
    public void EsBlanco_DetectaAusenciaDeDictado(string? codigo, bool esperado)
    {
        Assert.Equal(esperado, BloqueHorario.EsBlanco(codigo));
    }

    [Fact]
    public void ArmarSlots_DescartaDiasEnBlancoYMantieneLosConDictado()
    {
        var marcas = new (string, bool, bool, bool)[]
        {
            ("LUNES", true, false, false),
            ("MARTES", false, false, false),   // sin marcas → se descarta
            ("MIERCOLES", true, true, true),
        };

        var slots = HorarioComision.ArmarSlots(marcas);

        Assert.Equal(2, slots.Count);
        Assert.Equal(new HorarioComision.Slot("LUNES", "PRIMERO"), slots[0]);
        Assert.Equal(new HorarioComision.Slot("MIERCOLES", "UNICO"), slots[1]);
    }
}
