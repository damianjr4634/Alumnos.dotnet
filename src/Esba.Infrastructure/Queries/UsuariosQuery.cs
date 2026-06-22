using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.Common;
using Esba.Application.DTOs.Administracion;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Listado de usuarios del sistema, server-side (§3.2). Sucesor de la lectura de
/// AltaUsuario/BajaUsuarios. SUPERV/CAMPASS ('S'/'N') se proyectan a bool;
/// FECHA_BAJ distingue activos de dados de baja (baja lógica del hito 10.1a).
/// </summary>
public sealed class UsuariosQuery : IUsuariosQuery
{
    private const string ColumnasSelect = """
        SELECT CODUSU                                  AS Codigo,
               TRIM(NOMBRE)                            AS NombreUsuario,
               TRIM(NOMUSU)                            AS Nombres,
               TRIM(APELLIDO)                          AS Apellido,
               TRIM(CARGO)                             AS Cargo,
               CASE WHEN SUPERV = 'S' THEN 1 ELSE 0 END  AS EsSupervisor,
               CASE WHEN CAMPASS = 'S' THEN 1 ELSE 0 END AS DebeCambiarPassword,
               FECHA_BAJ                               AS FechaBaja
        """;

    private const string OrdenDefecto = "NOMBRE";

    private static readonly Dictionary<string, string> ColumnasOrdenables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["NombreUsuario"] = "NOMBRE",
            ["Apellido"] = "APELLIDO",
            ["Cargo"] = "CARGO",
        };

    private readonly FbConnectionFactory _connectionFactory;

    public UsuariosQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<UsuarioListItemDto>> BuscarAsync(UsuariosFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var parametros = new DynamicParameters();
        var where = ArmarWhere(filtro, parametros);
        var orderBy = ArmarOrderBy(filtro);

        var sqlItems = $"""
            {ColumnasSelect}
            FROM USUARIOS
            {where}
            ORDER BY {orderBy}
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        parametros.Add("Skip", filtro.Skip);
        parametros.Add("Take", filtro.Take);

        var sqlTotal = $"SELECT COUNT(*) FROM USUARIOS {where}";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var items = await connection.QueryAsync<UsuarioListItemDto>(
            new CommandDefinition(sqlItems, parametros, cancellationToken: ct)).ConfigureAwait(false);
        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sqlTotal, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new PagedResult<UsuarioListItemDto> { Items = items.AsList(), Total = total };
    }

    private static string ArmarWhere(UsuariosFiltro filtro, DynamicParameters parametros)
    {
        var condiciones = new List<string>();

        if (!filtro.IncluirBajas)
        {
            condiciones.Add("FECHA_BAJ IS NULL");
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add("(NOMBRE CONTAINING @Texto OR NOMUSU CONTAINING @Texto"
                + " OR APELLIDO CONTAINING @Texto OR CARGO CONTAINING @Texto)");
            parametros.Add("Texto", filtro.Texto.Trim());
        }

        return condiciones.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", condiciones);
    }

    private static string ArmarOrderBy(UsuariosFiltro filtro)
    {
        if (filtro.OrdenarPor is not null && ColumnasOrdenables.TryGetValue(filtro.OrdenarPor, out var columna))
        {
            var direccion = filtro.Descendente ? "DESC" : "ASC";
            return $"{columna} {direccion}, NOMBRE";
        }

        return OrdenDefecto;
    }
}
