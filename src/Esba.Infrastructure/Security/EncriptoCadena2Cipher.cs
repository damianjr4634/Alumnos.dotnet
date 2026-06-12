using System.Text;
using Esba.Application.Abstractions;

namespace Esba.Infrastructure.Security;

/// <summary>
/// Port fiel de EncriptoCadena2 (FuncionesText.pas:241): corrimiento por
/// carácter de +1/+3/+5 según la posición (contador mod 3) con ciclo de 14
/// caracteres; signo +1 cifra, -1 descifra. Solo para verificar contraseñas
/// legacy aún no re-hasheadas. // TODO-migrar: retirar al completar el re-hash.
/// </summary>
public sealed class EncriptoCadena2Cipher : ILegacyPasswordCipher
{
    public string Cifrar(string textoPlano) => Transformar(textoPlano, 1);

    public string Descifrar(string textoCifrado) => Transformar(textoCifrado, -1);

    private static string Transformar(string texto, int signo)
    {
        ArgumentNullException.ThrowIfNull(texto);

        var resultado = new StringBuilder(texto.Length);
        var cont = 1;
        foreach (var caracter in texto)
        {
            var corrimiento = (cont % 3) switch
            {
                0 => 5,
                1 => 1,
                _ => 3,
            };
            resultado.Append((char)(caracter + (corrimiento * signo)));

            cont++;
            if (cont == 15)
            {
                cont = 1;
            }
        }

        return resultado.ToString();
    }
}
