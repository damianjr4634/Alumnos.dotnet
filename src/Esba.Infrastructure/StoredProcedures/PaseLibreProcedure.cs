using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT ERRCOD, ERRMSG FROM XXX_FALTAS_PASLIBRE(@CodAlu, @Carre). El SP hace el
/// UPDATE de CURSADA (CURSANDO → LIBRES) y devuelve ERRCOD=1 (confirmación).
///
/// // TODO-migrar (prioridad baja): un UPDATE de una línea; portarlo es directo.
/// </summary>
public sealed class PaseLibreProcedure : IPaseLibreProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public PaseLibreProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<string>> EjecutarAsync(string codigoAlumno, string codigoCarrera, bool confirmar, CancellationToken ct)
    {
        const string sql = "SELECT ERRMSG FROM XXX_FALTAS_PASLIBRE(@CodAlu, @Carre)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var transaccion = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        var mensaje = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
            sql,
            new { CodAlu = codigoAlumno, Carre = codigoCarrera },
            transaction: transaccion,
            cancellationToken: ct)).ConfigureAwait(false);

        if (confirmar)
        {
            await transaccion.CommitAsync(ct).ConfigureAwait(false);
            return Result.Ok("Materias pasadas a LIBRE.");
        }

        await transaccion.RollbackAsync(ct).ConfigureAwait(false);
        return Result.NeedsConfirmation<string>(mensaje ?? "¿Pasar todas las materias del alumno a LIBRE?");
    }
}
