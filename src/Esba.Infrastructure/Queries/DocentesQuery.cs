using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

public sealed class DocentesQuery : IDocentesQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public DocentesQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<DocenteListItemDto>> ListarActivosAsync(CancellationToken ct)
    {
        // Legacy: SELECT CODPROFES, DOCENTE FROM DOCENTES WHERE FECHA_BAJ IS NULL ORDER BY 1.
        const string sql = """
            SELECT TRIM(CODPROFES) AS Codigo,
                   TRIM(DOCENTE)   AS Nombre
            FROM DOCENTES
            WHERE FECHA_BAJ IS NULL
            ORDER BY CODPROFES
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<DocenteListItemDto>(
            new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
