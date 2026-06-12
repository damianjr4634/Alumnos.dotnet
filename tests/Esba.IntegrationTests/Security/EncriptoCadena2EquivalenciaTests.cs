using Dapper;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Security;

namespace Esba.IntegrationTests.Security;

/// <summary>
/// Equivalencia del port de EncriptoCadena2 (FuncionesText.pas:241). Sin
/// conocer las contraseñas en claro, el cifrado reversible permite verificar
/// contra TODOS los PASSWD reales: Cifrar(Descifrar(x)) debe reproducir x.
/// </summary>
public class EncriptoCadena2EquivalenciaTests
{
    private readonly EncriptoCadena2Cipher _cipher = new();

    [Fact]
    public void RoundTrip_DescifrarLoCifrado_DevuelveElOriginal()
    {
        const string original = "Esba2026!";

        var cifrado = _cipher.Cifrar(original);

        Assert.NotEqual(original, cifrado);
        Assert.Equal(original, _cipher.Descifrar(cifrado));
    }

    [Fact]
    public void Cifrar_AplicaCorrimientosPorPosicion()
    {
        // Posiciones 1..3 → +1, +3, +5 (cont mod 3 = 1, 2, 0), igual que el Delphi.
        var cifrado = _cipher.Cifrar("AAA");

        Assert.Equal("BDF", cifrado);
    }

    [Fact]
    public void Cifrar_ElCicloSeReiniciaCada14Caracteres()
    {
        // El contador legacy vuelve a 1 al llegar a 15: el carácter 15 recibe
        // el mismo corrimiento (+1) que el primero.
        var cifrado = _cipher.Cifrar(new string('A', 15));

        Assert.Equal(cifrado[0], cifrado[14]);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task PasswdsReales_RoundTripDescifrarCifrar_ReproduceElValorAlmacenado()
    {
        var connectionString = Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
            ?? "database=localhost:/var/firebird/esba_restore.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";
        var factory = new FbConnectionFactory(connectionString);

        await using var connection = await factory.CreateOpenConnectionAsync(CancellationToken.None);
        var passwds = (await connection.QueryAsync<string>(
            "SELECT PASSWD FROM USUARIOS WHERE PASSWD NOT STARTING WITH '$E1$'")).AsList();

        Assert.NotEmpty(passwds);
        Assert.All(passwds, p => Assert.Equal(p, _cipher.Cifrar(_cipher.Descifrar(p))));
    }
}
