using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.Common;
using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

public sealed class MateriasQuery : IMateriasQuery
{
    private const string ColumnasSelect = """
        SELECT TRIM(CODMATERI) AS Codigo,
               TRIM(CODCARRE)  AS CodigoCarrera,
               TRIM(DESCRIPCI) AS Nombre,
               TRIM(SIGLA)     AS Sigla,
               CUATRIM         AS Cuatrimestre,
               IIF(ANUAL = 'S', TRUE, FALSE)     AS EsAnual,
               IIF(PROMOCION = 'S', TRUE, FALSE) AS AdmitePromocion,
               ORDEN           AS Orden
        FROM MATERIAS
        """;

    /// <summary>Orden por defecto, igual al del listado legacy.</summary>
    private const string OrdenDefecto = "CUATRIM, ORDEN, DESCRIPCI";

    /// <summary>
    /// Whitelist campo de orden → columna física. Evita inyección en el ORDER BY
    /// (el campo viene de la grilla; nunca se concatena el valor del usuario).
    /// </summary>
    private static readonly Dictionary<string, string> ColumnasOrdenables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Codigo"] = "CODMATERI",
            ["Nombre"] = "DESCRIPCI",
            ["Sigla"] = "SIGLA",
            ["Cuatrimestre"] = "CUATRIM",
            ["Orden"] = "ORDEN",
        };

    private readonly FbConnectionFactory _connectionFactory;

    public MateriasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MateriaListItemDto>> ListarPorCarreraAsync(string codigoCarrera, CancellationToken ct)
    {
        var sql = $"""
            {ColumnasSelect}
            WHERE CODCARRE = @Carre
            ORDER BY {OrdenDefecto}
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<MateriaListItemDto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }

    public async Task<PagedResult<MateriaListItemDto>> BuscarAsync(MateriasFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var parametros = new DynamicParameters();
        var where = ArmarWhere(filtro, parametros);
        var orderBy = ArmarOrderBy(filtro);

        var sqlItems = $"""
            {ColumnasSelect}
            {where}
            ORDER BY {orderBy}
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        parametros.Add("Skip", filtro.Skip);
        parametros.Add("Take", filtro.Take);

        var sqlTotal = $"SELECT COUNT(*) FROM MATERIAS {where}";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var items = await connection.QueryAsync<MateriaListItemDto>(
            new CommandDefinition(sqlItems, parametros, cancellationToken: ct)).ConfigureAwait(false);
        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sqlTotal, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new PagedResult<MateriaListItemDto> { Items = items.AsList(), Total = total };
    }

    /// <summary>
    /// Arma el WHERE: solo fragmentos SQL constantes; todo valor de usuario viaja
    /// como parámetro (§1.3, prohibida la concatenación de valores).
    /// </summary>
    private static string ArmarWhere(MateriasFiltro filtro, DynamicParameters parametros)
    {
        var condiciones = new List<string> { "CODCARRE = @Carre" };
        parametros.Add("Carre", filtro.CodigoCarrera);

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add("(DESCRIPCI CONTAINING @Texto OR SIGLA CONTAINING @Texto)");
            parametros.Add("Texto", filtro.Texto.Trim());
        }

        if (filtro.Cuatrimestre is { } cuatrimestre)
        {
            condiciones.Add("CUATRIM = @Cuatrimestre");
            parametros.Add("Cuatrimestre", cuatrimestre);
        }

        if (filtro.SoloAnuales is { } anuales)
        {
            condiciones.Add("ANUAL = @Anual");
            parametros.Add("Anual", anuales ? "S" : "N");
        }

        if (filtro.SoloConPromocion is { } promocion)
        {
            condiciones.Add("PROMOCION = @Promocion");
            parametros.Add("Promocion", promocion ? "S" : "N");
        }

        return "WHERE " + string.Join(" AND ", condiciones);
    }

    /// <summary>Orden estable a partir de la whitelist; cae al orden por defecto.</summary>
    private static string ArmarOrderBy(MateriasFiltro filtro)
    {
        if (filtro.OrdenarPor is not null && ColumnasOrdenables.TryGetValue(filtro.OrdenarPor, out var columna))
        {
            var direccion = filtro.OrdenDescendente ? "DESC" : "ASC";
            return $"{columna} {direccion}, CODMATERI";
        }

        return OrdenDefecto;
    }
}
