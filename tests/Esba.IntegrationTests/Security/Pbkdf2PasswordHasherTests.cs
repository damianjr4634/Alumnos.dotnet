using Esba.Infrastructure.Security;

namespace Esba.IntegrationTests.Security;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_EntraEnPasswdVarchar60()
    {
        var hash = _hasher.Hash("una contraseña cualquiera");

        Assert.Equal(60, hash.Length);
        Assert.StartsWith("$E1$", hash, StringComparison.Ordinal);
    }

    [Fact]
    public void Verify_PasswordCorrecta_DevuelveTrue()
    {
        var hash = _hasher.Hash("clave segura");

        Assert.True(_hasher.Verify(hash, "clave segura"));
    }

    [Fact]
    public void Verify_PasswordIncorrecta_DevuelveFalse()
    {
        var hash = _hasher.Hash("clave segura");

        Assert.False(_hasher.Verify(hash, "clave Segura"));
    }

    [Fact]
    public void Hash_MismaPassword_GeneraHashesDistintosPorSalt()
    {
        Assert.NotEqual(_hasher.Hash("clave"), _hasher.Hash("clave"));
    }

    [Fact]
    public void CanVerify_CifradoLegacy_DevuelveFalse()
    {
        Assert.False(_hasher.CanVerify("dpthb"));
    }
}
