using System.Security.Cryptography;
using Esba.Application.Abstractions;

namespace Esba.Infrastructure.Security;

/// <summary>
/// PBKDF2-SHA256 con formato compacto "$E1$" + Base64(salt[16] + hash[24]) =
/// 60 caracteres exactos: el máximo de USUARIOS.PASSWD VARCHAR(60), que no se
/// puede ensanchar mientras la base se comparta con el Delphi legacy. El
/// prefijo "$E1$" distingue el formato nuevo del cifrado legacy (una salida de
/// EncriptoCadena2 no puede empezar así para contraseñas reales).
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefijo = "$E1$";
    private const int Iteraciones = 210_000;
    private const int LargoSalt = 16;
    private const int LargoHash = 24;

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(LargoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, LargoHash);

        var payload = new byte[LargoSalt + LargoHash];
        salt.CopyTo(payload, 0);
        hash.CopyTo(payload, LargoSalt);

        return Prefijo + Convert.ToBase64String(payload);
    }

    public bool CanVerify(string stored) =>
        stored is not null && stored.StartsWith(Prefijo, StringComparison.Ordinal);

    public bool Verify(string stored, string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        if (!CanVerify(stored))
        {
            return false;
        }

        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(stored[Prefijo.Length..]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (payload.Length != LargoSalt + LargoHash)
        {
            return false;
        }

        var salt = payload.AsSpan(0, LargoSalt).ToArray();
        var esperado = payload.AsSpan(LargoSalt).ToArray();
        var calculado = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, LargoHash);

        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }
}
