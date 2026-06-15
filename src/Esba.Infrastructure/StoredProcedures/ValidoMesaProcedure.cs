using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT ERRCOD, ERRMSG FROM XXX_VALIDO_MESA(@Mesa, @Carre). Pre-chequeo de
/// duplicado en el alta de mesas.
///
/// // TODO-migrar (prioridad baja): EXISTS sobre MESAS por (MESA, CARRE).
/// </summary>
public sealed class ValidoMesaProcedure : IValidoMesaProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public ValidoMesaProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<bool>> VerificarAsync(int numeroMesa, string codigoCarrera, CancellationToken ct)
    {
        const string sql = "SELECT ERRCOD, ERRMSG FROM XXX_VALIDO_MESA(@Mesa, @Carre)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QueryFirstOrDefaultAsync<(int ErrCod, string? ErrMsg)>(new CommandDefinition(
            sql, new { Mesa = numeroMesa, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return Result.DesdeErrCod(fila.ErrCod, fila.ErrMsg, true);
    }
}
