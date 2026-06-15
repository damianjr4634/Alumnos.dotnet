using System.Data;
using Dapper;
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

    static FbConnectionFactory()
    {
        // Firebird devuelve DATE como DateTime; este handler permite proyectar a
        // DateOnly en queries y wrappers Dapper. Se registra una sola vez (todas
        // las lecturas Dapper pasan por esta fábrica, también en los tests).
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

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

    private sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }
    }
}
