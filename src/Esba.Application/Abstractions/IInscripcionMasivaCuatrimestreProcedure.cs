using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>Parámetros de XXX_INSC_CUAT_16032023.</summary>
public sealed record InscripcionMasivaParametros
{
    public required string CodigoAlumno { get; init; }

    public required short Curso { get; init; }

    public required string CodigoCarrera { get; init; }

    public required string CuatrimestreAnio { get; init; }

    public string? Instituto { get; init; }

    public string? Caracteristica { get; init; }

    public required int CodigoUsuario { get; init; }
}

/// <summary>
/// Wrapper de XXX_INSC_CUAT_16032023 (inscripción masiva por cuatrimestre). El SP
/// inserta en CURSADA y devuelve FERRCOD/FERRMSG. Soporta el patrón de dos fases
/// sin transacción de larga vida: con <paramref name="confirmar"/>=false ejecuta
/// y hace rollback (previsualización); con true commitea salvo error (FERRCOD=2).
/// </summary>
public interface IInscripcionMasivaCuatrimestreProcedure
{
    Task<Result<string>> EjecutarAsync(InscripcionMasivaParametros parametros, bool confirmar, CancellationToken ct);
}
