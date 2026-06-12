using Esba.Domain.Common;

namespace Esba.Application.Abstractions;

/// <summary>
/// Parámetros del SP XXX_CAMBIA_DNI_LM, fieles al PSQL
/// (db/schema/procedures/XXX_CAMBIA_DNI_LM.sql).
/// </summary>
public sealed record CambioDniLibroMatrizParametros
{
    /// <summary>CODALU CHAR(11): código actual del alumno.</summary>
    public required string CodigoAlumno { get; init; }

    /// <summary>CARRE VARCHAR(6).</summary>
    public required string CodigoCarrera { get; init; }

    /// <summary>TIPO CHAR(1): 'L' cambia libro matriz, 'D' cambia código de alumno.</summary>
    public required char Tipo { get; init; }

    /// <summary>LM CHAR(5): nuevo libro matriz (solo TIPO='L').</summary>
    public string? NuevoLibroMatriz { get; init; }

    /// <summary>N_CODALU CHAR(11): nuevo código de alumno (solo TIPO='D').</summary>
    public string? NuevoCodigoAlumno { get; init; }
}

/// <summary>
/// Wrapper tipado del SP legacy XXX_CAMBIA_DNI_LM (usado por
/// CambioCodAlu_LM.pas): cambia libro matriz o código de alumno validando que
/// el nuevo valor no pertenezca a otro alumno y propagando el cambio a
/// ALUMNOS/ANALITIC/CURSADA/RECURSA. La semántica FERRCOD/FERRMSG queda
/// encapsulada acá: el llamador solo ve Result&lt;string&gt; (el mensaje del SP).
/// </summary>
public interface ICambioDniLibroMatrizProcedure
{
    Task<Result<string>> EjecutarAsync(CambioDniLibroMatrizParametros parametros, CancellationToken ct);
}
