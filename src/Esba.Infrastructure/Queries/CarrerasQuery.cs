using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Carreras;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

public sealed class CarrerasQuery : ICarrerasQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public CarrerasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CarreraListItemDto>> ListarParaMenuAsync(
        IReadOnlyCollection<string>? codigosPermitidos, CancellationToken ct)
    {
        // No supervisor sin permisos en BARRA_SEGU: menú vacío, sin ir a la base
        // (Dapper expande una lista IN vacía a SQL que Firebird no acepta).
        if (codigosPermitidos is { Count: 0 })
        {
            return [];
        }

        // GRUPO referencia CARRE_GRP (descripciones de grupo aún no migradas).
        // TODO-migrar CARRE_GRP: hoy el menú muestra el código de grupo.
        var sql = """
            SELECT TRIM(CARRE)   AS Codigo,
                   TRIM(DESCARRE) AS Nombre,
                   TRIM(DESCORT)  AS NombreCorto,
                   TRIM(GRUPO)    AS Grupo,
                   ORDEN          AS Orden,
                   IIF(DISTANCIA = 'S', TRUE, FALSE) AS EsADistancia,
                   IIF(DESACT = 'S', TRUE, FALSE)    AS Desactivada
            FROM CARRERA
            WHERE DESACT = 'N'
            """;

        if (codigosPermitidos is not null)
        {
            sql += " AND TRIM(CARRE) IN @Codigos";
        }

        sql += " ORDER BY GRUPO, ORDEN, DESCARRE";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var carreras = await connection.QueryAsync<CarreraListItemDto>(
            new CommandDefinition(sql, new { Codigos = codigosPermitidos }, cancellationToken: ct)).ConfigureAwait(false);

        return carreras.AsList();
    }
}
