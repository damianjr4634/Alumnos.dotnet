using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT ERRCOD, ERRMSG FROM XXX_VALIDO_COMISION(@CuaAnio,@CodMat,@Cutuco,@Carre).
/// Pre-chequeo de duplicado en el alta de comisiones.
///
/// // TODO-migrar (prioridad baja): el PSQL solo verifica si ya existe una fila en
/// // COMARM con esa PK; portarlo a C# es un EXISTS trivial sobre el repositorio.
/// </summary>
public sealed class ValidoComisionProcedure : IValidoComisionProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public ValidoComisionProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<bool>> VerificarAsync(
        string cuatrimestreAnio, string codigoMateria, short cutuco, string codigoCarrera, CancellationToken ct)
    {
        const string sql = "SELECT ERRCOD, ERRMSG FROM XXX_VALIDO_COMISION(@CuaAnio, @CodMat, @Cutuco, @Carre)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QueryFirstOrDefaultAsync<(int ErrCod, string? ErrMsg)>(new CommandDefinition(
            sql,
            new
            {
                CuaAnio = cuatrimestreAnio,
                CodMat = codigoMateria,
                Cutuco = cutuco.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Carre = codigoCarrera,
            },
            cancellationToken: ct)).ConfigureAwait(false);

        return Result.DesdeErrCod(fila.ErrCod, fila.ErrMsg, true);
    }
}
