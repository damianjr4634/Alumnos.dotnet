using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Entities;
using Esba.Domain.Enums;

namespace Esba.Application.Features.AreaDocente;

/// <summary>
/// Reglas de quién puede qué sobre una precarga (decisiones 2026-10-03), compartidas por
/// los handlers de comisiones y de mesas. El servidor decide: la UI solo oculta botones.
/// - Secretaría: siempre (editar, finalizar, reabrir, efectivizar), en cualquier estado.
/// - Docente: solo si es el TITULAR (COMARM.CODPROFES / MESAS.TITULAR) y la carga está en
///   borrador (o no existe todavía). Puede finalizar; no puede reabrir.
/// </summary>
internal static class AutorizacionCargaDocente
{
    /// <summary>Mensaje de error si el actor no puede editar; null si puede.</summary>
    public static string? MotivoNoPuedeEditar(ActorCargaDocente actor, string? codigoDocenteTitular, EstadoCargaDocente? estado, string queCosa = "esta comisión")
    {
        if (actor.EsSecretaria)
        {
            return null;
        }

        if (!EsTitular(actor, codigoDocenteTitular))
        {
            return $"Solo el docente titular puede cargar {queCosa}.";
        }

        return estado switch
        {
            null or EstadoCargaDocente.Borrador => null,
            EstadoCargaDocente.Finalizada => "La carga está finalizada: pedile a secretaría que la reabra si necesitás corregir algo.",
            EstadoCargaDocente.Efectivizada => "La carga ya fue efectivizada por secretaría y no se puede modificar.",
            _ => "La carga no se puede modificar.",
        };
    }

    /// <summary>Mensaje de error si el actor no puede finalizar; null si puede.</summary>
    public static string? MotivoNoPuedeFinalizar(ActorCargaDocente actor, ICargaDocente carga)
    {
        if (!actor.EsSecretaria && !EsTitular(actor, carga.CodigoDocente))
        {
            return "Solo el docente titular puede finalizar esta carga.";
        }

        return carga.Estado switch
        {
            EstadoCargaDocente.Borrador => null,
            EstadoCargaDocente.Finalizada => "La carga ya está finalizada.",
            EstadoCargaDocente.Efectivizada => "La carga ya fue efectivizada.",
            _ => "La carga no se puede finalizar.",
        };
    }

    /// <summary>Mensaje de error si el actor no puede reabrir; null si puede. Solo secretaría.</summary>
    public static string? MotivoNoPuedeReabrir(ActorCargaDocente actor, ICargaDocente carga)
    {
        if (!actor.EsSecretaria)
        {
            return "Solo secretaría puede reabrir una carga finalizada.";
        }

        return carga.Estado == EstadoCargaDocente.Borrador ? "La carga ya está en borrador." : null;
    }

    /// <summary>Mensaje de error si el actor no puede efectivizar; null si puede. Solo secretaría, y no dos veces.</summary>
    public static string? MotivoNoPuedeEfectivizar(ActorCargaDocente actor, ICargaDocente carga)
    {
        if (!actor.EsSecretaria)
        {
            return "Solo secretaría puede efectivizar una precarga.";
        }

        return carga.Estado == EstadoCargaDocente.Efectivizada ? "La carga ya está efectivizada." : null;
    }

    public static bool EsTitular(ActorCargaDocente actor, string? codigoDocenteTitular) =>
        actor.CodigoDocente is not null && codigoDocenteTitular is not null
        && string.Equals(actor.CodigoDocente.Trim(), codigoDocenteTitular.Trim(), StringComparison.OrdinalIgnoreCase);
}
