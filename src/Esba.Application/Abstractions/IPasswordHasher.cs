namespace Esba.Application.Abstractions;

/// <summary>
/// Hash de contraseñas del sistema nuevo (PBKDF2, migration_improvements.md §2.7).
/// El formato debe entrar en USUARIOS.NPASSWD VARCHAR(60) (columna propia del
/// login web; PASSWD queda para el cifrado legacy del escritorio).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string stored, string password);

    /// <summary>True si el valor almacenado tiene el formato nuevo; false si es el cifrado legacy.</summary>
    bool CanVerify(string stored);
}
