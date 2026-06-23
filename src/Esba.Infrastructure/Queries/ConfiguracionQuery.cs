using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Listado de la configuración del sistema (XXX_CONF), sucesor de la lectura de
/// TablaConfiguraciones.pas. SQL parametrizado, sin concatenación (§1.3).
/// </summary>
public sealed class ConfiguracionQuery : IConfiguracionQuery
{
    private const string Sql = """
        SELECT TRIM(PARAME) AS Parame,
               DESCRI        AS Descripcion,
               VALOR         AS Valor
        FROM XXX_CONF
        ORDER BY PARAME
        """;

    private readonly FbConnectionFactory _connectionFactory;

    public ConfiguracionQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ParametroConfiguracionDto>> ListarAsync(CancellationToken ct)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var items = await connection.QueryAsync<ParametroConfiguracionDto>(
            new CommandDefinition(Sql, cancellationToken: ct)).ConfigureAwait(false);

        return items.AsList();
    }
}
