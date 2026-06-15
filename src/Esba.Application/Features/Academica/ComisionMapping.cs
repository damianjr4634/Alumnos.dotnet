using Esba.Application.DTOs.Academica;
using Esba.Domain.Academica;
using Esba.Domain.Entities;

namespace Esba.Application.Features.Academica;

/// <summary>
/// Mapeo del horario (marcas por día) a las columnas COMARM.DIAn/BLOQUEn,
/// común al alta y a la modificación. Usa la lógica de dominio
/// <see cref="HorarioComision"/>/<see cref="BloqueHorario"/>.
/// </summary>
internal static class ComisionMapping
{
    public static void AplicarHorario(Comision comision, IReadOnlyList<HorarioDiaComision> horario)
    {
        var slots = HorarioComision.ArmarSlots(
            horario.Select(h => (h.Dia, h.Primero, h.Segundo, h.Tercero)));

        var (dia1, bloque1) = SlotEn(slots, 0);
        var (dia2, bloque2) = SlotEn(slots, 1);
        var (dia3, bloque3) = SlotEn(slots, 2);

        comision.Dia1 = dia1;
        comision.Bloque1 = bloque1;
        comision.Dia2 = dia2;
        comision.Bloque2 = bloque2;
        comision.Dia3 = dia3;
        comision.Bloque3 = bloque3;
    }

    private static (string Dia, string Bloque) SlotEn(IReadOnlyList<HorarioComision.Slot> slots, int indice) =>
        indice < slots.Count
            ? (slots[indice].Dia, slots[indice].Bloque)
            : (BloqueHorario.Blanco, BloqueHorario.Blanco);
}
