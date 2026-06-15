using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Tipos de mesa aplicables a la carrera. Reescritura del lookup de
/// MesasExamen.CbTipoButtonClick (MESA_TIPO filtrado por el TIPO de la carrera).
/// </summary>
public sealed class TipoMesaQuery : ITipoMesaQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public TipoMesaQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<TipoMesaDto>> ListarPorCarreraAsync(string codigoCarrera, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(CODIGO) AS Codigo,
                   TRIM(DESCRI) AS Descripcion
            FROM MESA_TIPO
            WHERE CARRE CONTAINING (SELECT TRIM(C.TIPO) FROM CARRERA C WHERE C.CARRE = @Carre)
            ORDER BY CODIGO
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<TipoMesaDto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
