namespace Esba.Domain.Academica;

/// <summary>
/// Codificación de los bloques horarios de una comisión por día, equivalente a
/// la grilla de cargacomisiones.pas: por cada día se marcan hasta 3 bloques
/// (1º/2º/3º) y se comprimen en un único código almacenado en COMARM.BLOQUEn.
/// Lógica de dominio pura (sucesora del armado disperso del formulario legacy).
/// </summary>
public static class BloqueHorario
{
    public const string Blanco = "BLANCO";

    /// <summary>
    /// Convierte el conjunto de bloques marcados (1, 2 y/o 3) de un día en el
    /// código legacy. Sin bloques → "BLANCO". Los tres → "UNICO".
    /// </summary>
    public static string Codificar(bool primero, bool segundo, bool tercero) =>
        (primero, segundo, tercero) switch
        {
            (true, true, true) => "UNICO",
            (true, true, false) => "PRISEG",
            (true, false, true) => "PRITER",
            (false, true, true) => "SEGTER",
            (true, false, false) => "PRIMERO",
            (false, true, false) => "SEGUNDO",
            (false, false, true) => "TERCERO",
            _ => Blanco,
        };

    /// <summary>
    /// Inversa de <see cref="Codificar"/>: del código legacy a qué bloques (1/2/3)
    /// quedan marcados. Un código desconocido o "BLANCO" deja todo en false.
    /// </summary>
    public static (bool Primero, bool Segundo, bool Tercero) Decodificar(string? codigo) =>
        codigo?.Trim().ToUpperInvariant() switch
        {
            "UNICO" => (true, true, true),
            "PRISEG" => (true, true, false),
            "PRITER" => (true, false, true),
            "SEGTER" => (false, true, true),
            "PRIMERO" => (true, false, false),
            "SEGUNDO" => (false, true, false),
            "TERCERO" => (false, false, true),
            _ => (false, false, false),
        };

    /// <summary>true si el día/código no representa dictado (null, vacío o "BLANCO").</summary>
    public static bool EsBlanco(string? codigo)
    {
        var c = codigo?.Trim();
        return string.IsNullOrEmpty(c) || string.Equals(c, Blanco, StringComparison.OrdinalIgnoreCase);
    }
}
