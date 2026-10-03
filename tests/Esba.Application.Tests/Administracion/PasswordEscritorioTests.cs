using Esba.Application.Features.Administracion;

namespace Esba.Application.Tests.Administracion;

/// <summary>
/// El sentinela de PASSWD para docentes/alumnos debe caber en VARCHAR(60), ser
/// reconocible y distinto por usuario (no hay un valor compartido adivinable).
/// </summary>
public class PasswordEscritorioTests
{
    [Fact]
    public void GenerarBloqueo_EmpiezaConElMarcadorYCabeEnLaColumna()
    {
        var sentinela = PasswordEscritorio.GenerarBloqueo();

        Assert.StartsWith(PasswordEscritorio.MarcadorBloqueo, sentinela, StringComparison.Ordinal);
        Assert.InRange(sentinela.Length, PasswordEscritorio.MarcadorBloqueo.Length + 16, 60);
    }

    [Fact]
    public void GenerarBloqueo_DosLlamadas_DanValoresDistintos() =>
        Assert.NotEqual(PasswordEscritorio.GenerarBloqueo(), PasswordEscritorio.GenerarBloqueo());

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("/", false)]            // blanqueo legacy
    [InlineData("cifradoLegacy", false)]
    [InlineData("$E1$hash", false)]
    [InlineData("#WEB#abc", true)]
    public void EstaBloqueado_DistingueSentinelaDeCifradoLegacy(string? passwd, bool esperado) =>
        Assert.Equal(esperado, PasswordEscritorio.EstaBloqueado(passwd));
}
