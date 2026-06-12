using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT * FROM XXX_CAMBIA_DNI_LM(...) vía Dapper. El SP se CONSERVA en esta
/// fase (regla 🔴 §1.3); este wrapper es el único punto de invocación.
///
/// // TODO-migrar (prioridad media): la lógica PSQL valida que el nuevo libro
/// // matriz/documento no pertenezca a otro alumno de la carrera y, si está
/// // libre, propaga el cambio a ALUMNOS, ANALITIC, CURSADA y RECURSA en una
/// // transacción. Portarla a C# requiere los modelos de ANALITIC/RECURSA y
/// // tests de equivalencia (fase 5 de retiro de SPs).
/// </summary>
public sealed class CambioDniLibroMatrizProcedure : ICambioDniLibroMatrizProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public CambioDniLibroMatrizProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<string>> EjecutarAsync(CambioDniLibroMatrizParametros parametros, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        // El PSQL ignora silenciosamente un TIPO desconocido (devolvería FERRCOD=0
        // sin hacer nada): se corta acá, es un error de programación del llamador.
        switch (parametros.Tipo)
        {
            case 'L' when string.IsNullOrWhiteSpace(parametros.NuevoLibroMatriz):
                throw new ArgumentException("TIPO='L' requiere NuevoLibroMatriz.", nameof(parametros));
            case 'D' when string.IsNullOrWhiteSpace(parametros.NuevoCodigoAlumno):
                throw new ArgumentException("TIPO='D' requiere NuevoCodigoAlumno.", nameof(parametros));
            case 'L' or 'D':
                break;
            default:
                throw new ArgumentException($"TIPO inválido: '{parametros.Tipo}' (se espera 'L' o 'D').", nameof(parametros));
        }

        const string sql = """
            SELECT FERRCOD, FERRMSG
            FROM XXX_CAMBIA_DNI_LM(@CodAlu, @Lm, @NuevoCodAlu, @Tipo, @Carre)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QuerySingleAsync<FilaResultado>(new CommandDefinition(
            sql,
            new
            {
                CodAlu = parametros.CodigoAlumno,
                Lm = parametros.NuevoLibroMatriz,
                NuevoCodAlu = parametros.NuevoCodigoAlumno,
                Tipo = parametros.Tipo.ToString(),
                Carre = parametros.CodigoCarrera,
            },
            cancellationToken: ct)).ConfigureAwait(false);

        // Mapeo FERRCOD/FERRMSG → Result (§1.3). Nota: el camino exitoso del SP
        // devuelve 0 CON mensaje ("Se cambio...") → Warning, fiel a la semántica
        // de ExecScriptMsg (aviso informativo, la operación se completó).
        var mensaje = fila.FERRMSG?.TrimEnd();
        return Result.DesdeErrCod(fila.FERRCOD, mensaje, mensaje ?? string.Empty);
    }

    /// <summary>Fila de retorno fiel al RETURNS del PSQL.</summary>
    private sealed record FilaResultado(int? FERRCOD, string? FERRMSG);
}
