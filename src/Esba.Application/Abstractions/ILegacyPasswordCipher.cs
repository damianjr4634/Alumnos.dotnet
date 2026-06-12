namespace Esba.Application.Abstractions;

/// <summary>
/// Cifrado reversible legacy de contraseñas (EncriptoCadena2 de
/// FuncionesText.pas). Existe SOLO para verificar contraseñas aún no
/// re-hasheadas durante la transición. // TODO-migrar: retirar cuando todos
/// los usuarios tengan hash nuevo.
/// </summary>
public interface ILegacyPasswordCipher
{
    string Cifrar(string textoPlano);

    string Descifrar(string textoCifrado);
}
