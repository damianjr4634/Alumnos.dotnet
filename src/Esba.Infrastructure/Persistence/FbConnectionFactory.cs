using FirebirdSql.Data.FirebirdClient;

namespace Esba.Infrastructure.Persistence;

/// <summary>
/// Fábrica de conexiones para las queries Dapper. Cada lectura abre y cierra su
/// propia conexión: no hay conexión compartida de larga vida (anti-patrón del
/// god datamodule legacy, migration_improvements.md §1.2.4).
/// </summary>
public sealed class FbConnectionFactory
{
    private readonly string _connectionString;

    public FbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<FbConnection> CreateOpenConnectionAsync(CancellationToken ct)
    {
        var connection = new FbConnection(_connectionString);
        try
        {
            await connection.OpenAsync(ct).ConfigureAwait(false);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
