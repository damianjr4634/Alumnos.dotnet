using Esba.Application.DTOs.Academica;

namespace Esba.Application.Abstractions;

/// <summary>
/// Numeración de actuaciones de equivalencia, sucesor de <c>XXX_NUMERO_EQUIVALENCIA</c>
/// (sugiere el próximo número) y <c>XXX_GRABA_NUMEQUI</c> (lo confirma en TBLEQUIVA al
/// usar un número nuevo). El número vive en TBLEQUIVA por carrera (TER comparte secuencia).
/// </summary>
public interface IEquivalenciaNumeracionProcedure
{
    /// <summary>Próximo número sugerido para el alumno/carrera (XXX_NUMERO_EQUIVALENCIA).</summary>
    Task<NumeroEquivalenciaDto> ObtenerProximoNumeroAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct);

    /// <summary>Confirma el número consumido en TBLEQUIVA (XXX_GRABA_NUMEQUI). Solo al usar numeración nueva interna.</summary>
    Task GrabarNumeroAsync(int numero, string codigoCarrera, CancellationToken ct);
}
