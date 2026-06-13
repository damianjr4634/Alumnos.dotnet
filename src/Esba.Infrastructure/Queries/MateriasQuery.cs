using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

public sealed class MateriasQuery : IMateriasQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public MateriasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MateriaListItemDto>> ListarPorCarreraAsync(string codigoCarrera, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(CODMATERI) AS Codigo,
                   TRIM(CODCARRE)  AS CodigoCarrera,
                   TRIM(DESCRIPCI) AS Nombre,
                   TRIM(SIGLA)     AS Sigla,
                   CUATRIM         AS Cuatrimestre,
                   IIF(ANUAL = 'S', TRUE, FALSE)     AS EsAnual,
                   IIF(PROMOCION = 'S', TRUE, FALSE) AS AdmitePromocion,
                   ORDEN           AS Orden
            FROM MATERIAS
            WHERE CODCARRE = @Carre
            ORDER BY CUATRIM, ORDEN, DESCRIPCI
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<MateriaListItemDto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
