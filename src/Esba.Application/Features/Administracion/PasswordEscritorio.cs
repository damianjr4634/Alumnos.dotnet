using System.Security.Cryptography;

namespace Esba.Application.Features.Administracion;

/// <summary>
/// Política de la columna USUARIOS.PASSWD (la que lee el ESBA de escritorio
/// Delphi, sesion.pas + EncriptoCadena2) para los usuarios que NO deben entrar
/// al escritorio: docentes y alumnos (decisión 2026-10-02). El Delphi no conoce
/// USUARIOS.TIPO y acepta a cualquier fila cuya PASSWD descifre a lo tipeado, así
/// que para esos perfiles PASSWD lleva un sentinela: un marcador fijo más un
/// sufijo aleatorio por usuario, cuyo "descifrado" es una cadena larga y
/// aleatoria que nadie puede tipear. Es la misma técnica del blanqueo legacy
/// ('/' + CAMPASS='S'), pero sin un valor adivinable.
/// Solo el personal de secretaría (<c>Usuario.UsaEscritorio</c>) sincroniza PASSWD
/// con el cifrado legacy de su contraseña real.
/// // TODO-migrar: desaparece junto con PASSWD al retirar el escritorio.
/// </summary>
public static class PasswordEscritorio
{
    /// <summary>Prefijo que identifica un PASSWD bloqueado (no es un cifrado legacy válido de nada útil).</summary>
    public const string MarcadorBloqueo = "#WEB#";

    private const int LargoSufijo = 32; // 5 + 32 = 37 ≤ VARCHAR(60)
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

    /// <summary>Genera un sentinela nuevo (aleatorio por usuario).</summary>
    public static string GenerarBloqueo() =>
        MarcadorBloqueo + RandomNumberGenerator.GetString(Alfabeto, LargoSufijo);

    /// <summary>true si el PASSWD dado es un sentinela de bloqueo (y no un cifrado legacy).</summary>
    public static bool EstaBloqueado(string? passwordLegacy) =>
        passwordLegacy is not null && passwordLegacy.StartsWith(MarcadorBloqueo, StringComparison.Ordinal);
}
