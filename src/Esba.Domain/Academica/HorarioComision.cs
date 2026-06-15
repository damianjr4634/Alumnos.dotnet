namespace Esba.Domain.Academica;

/// <summary>
/// Armado de los hasta 3 slots día/bloque de una comisión (COMARM.DIAn/BLOQUEn)
/// a partir de las marcas de la grilla, equivalente al recorrido de días de
/// cargacomisiones.GrabaMateriaClick. Lógica de dominio pura y testeable.
/// </summary>
public static class HorarioComision
{
    /// <summary>Días de la semana en el orden de la grilla legacy (columnas 1..5).</summary>
    public static readonly IReadOnlyList<string> Dias = ["LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES"];

    /// <summary>Slot de horario: día + código de bloque (ya comprimido).</summary>
    public readonly record struct Slot(string Dia, string Bloque);

    /// <summary>
    /// Convierte las marcas (día → bloques 1/2/3) en los slots con dictado,
    /// descartando los días en blanco. El llamador valida que no superen 3
    /// (la comisión solo tiene DIA1..DIA3).
    /// </summary>
    public static IReadOnlyList<Slot> ArmarSlots(IEnumerable<(string Dia, bool Primero, bool Segundo, bool Tercero)> marcas) =>
        marcas
            .Select(m => new Slot(m.Dia, BloqueHorario.Codificar(m.Primero, m.Segundo, m.Tercero)))
            .Where(s => !BloqueHorario.EsBlanco(s.Bloque))
            .ToList();
}
