using System.Globalization;

namespace Esba.Domain.Academica;

/// <summary>
/// CUA_ANIO (CHAR(3)): código de período lectivo del legacy, "cuatrimestre + año en
/// dos dígitos" ("226" = 2º cuatrimestre 2026, "124" = 1º cuatrimestre 2024). Los
/// datos reales traen también códigos fuera de ese patrón (p. ej. "505", "300"), que
/// se muestran tal cual. Sucesor parcial de FuncionesText.Cuatrimestre.
/// </summary>
public static class CuatrimestreAnio
{
    /// <summary>"2º cuatrimestre 2026" para "226"; el código sin cambios si no responde al patrón.</summary>
    public static string Etiqueta(string? cuaAnio)
    {
        var limpio = cuaAnio?.Trim();
        if (!TryDescomponer(limpio, out var cuatrimestre, out var anio))
        {
            return limpio ?? string.Empty;
        }

        return string.Create(CultureInfo.InvariantCulture, $"{cuatrimestre}º cuatrimestre {anio}");
    }

    /// <summary>
    /// Clave de orden cronológico (año, cuatrimestre): "226" → 202602. Los códigos que no
    /// responden al patrón ordenan al final (0), así no se mezclan con los períodos reales.
    /// </summary>
    public static int ClaveOrden(string? cuaAnio) =>
        TryDescomponer(cuaAnio?.Trim(), out var cuatrimestre, out var anio) ? (anio * 100) + cuatrimestre : 0;

    private static bool TryDescomponer(string? cuaAnio, out int cuatrimestre, out int anio)
    {
        cuatrimestre = 0;
        anio = 0;
        if (cuaAnio is not { Length: 3 } || !char.IsAsciiDigit(cuaAnio[0])
            || !char.IsAsciiDigit(cuaAnio[1]) || !char.IsAsciiDigit(cuaAnio[2]))
        {
            return false;
        }

        cuatrimestre = cuaAnio[0] - '0';
        if (cuatrimestre is < 1 or > 2)
        {
            return false;
        }

        anio = 2000 + int.Parse(cuaAnio.AsSpan(1, 2), CultureInfo.InvariantCulture);
        return true;
    }
}
