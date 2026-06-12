namespace Esba.Application.Abstractions;

/// <summary>
/// Hash de contraseñas del sistema nuevo (PBKDF2, migration_improvements.md §2.7).
/// El formato debe entrar en USUARIOS.PASSWD VARCHAR(60), compartida con el
/// legacy durante la transición.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string stored, string password);

    /// <summary>True si el valor almacenado tiene el formato nuevo; false si es el cifrado legacy.</summary>
    bool CanVerify(string stored);
}
