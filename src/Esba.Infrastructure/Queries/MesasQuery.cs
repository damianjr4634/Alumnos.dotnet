using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.Common;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Listado de mesas de examen. Reescritura parametrizada del SELECT de
/// MesasExamen.FormActivate (MESAS + LEFT JOIN MATERIAS + LEFT JOIN MESA_TIPO),
/// con paginación y orden server-side (§3.2).
/// </summary>
public sealed class MesasQuery : IMesasQuery
{
    private const string ColumnasSelect = """
        SELECT TRIM(M.CARRE)   AS CodigoCarrera,
               M.MESA          AS NumeroMesa,
               TRIM(M.COD_MAT) AS CodigoMateria,
               TRIM(A.SIGLA)   AS SiglaMateria,
               M.LLAMADO       AS Llamado,
               M.FECH_EXA      AS FechaExamen,
               M.HORA          AS Hora,
               TRIM(M.TITULAR) AS Titular,
               TRIM(M.VOCAL1)  AS Vocal1,
               TRIM(M.VOCAL2)  AS Vocal2,
               M.AULA          AS Aula,
               M.CUATRIM       AS Cuatrimestre,
               M.COMI1         AS Comision1,
               M.COMI2         AS Comision2,
               M.COMI3         AS Comision3,
               TRIM(M.TIPMES)  AS CodigoTipo,
               TRIM(T.DESCRI)  AS DescripcionTipo
        """;

    private const string FromYJoins = """
        FROM MESAS M
        LEFT OUTER JOIN MATERIAS A ON A.CODMATERI = M.COD_MAT AND A.CODCARRE = M.CARRE
        LEFT OUTER JOIN MESA_TIPO T ON T.CODIGO = M.TIPMES
        """;

    private const string OrdenDefecto = "M.MESA, M.COD_MAT";

    private static readonly Dictionary<string, string> ColumnasOrdenables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["NumeroMesa"] = "M.MESA",
            ["CodigoMateria"] = "M.COD_MAT",
            ["SiglaMateria"] = "A.SIGLA",
            ["FechaExamen"] = "M.FECH_EXA",
            ["Llamado"] = "M.LLAMADO",
        };

    private readonly FbConnectionFactory _connectionFactory;

    public MesasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<MesaListItemDto>> BuscarAsync(MesasFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var parametros = new DynamicParameters();
        var where = ArmarWhere(filtro, parametros);
        var orderBy = ArmarOrderBy(filtro);

        var sqlItems = $"""
            {ColumnasSelect}
            {FromYJoins}
            {where}
            ORDER BY {orderBy}
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        parametros.Add("Skip", filtro.Skip);
        parametros.Add("Take", filtro.Take);

        var sqlTotal = $"SELECT COUNT(*) {FromYJoins} {where}";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var items = await connection.QueryAsync<MesaListItemDto>(
            new CommandDefinition(sqlItems, parametros, cancellationToken: ct)).ConfigureAwait(false);
        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sqlTotal, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new PagedResult<MesaListItemDto> { Items = items.AsList(), Total = total };
    }

    public async Task<MesaDetailDto?> ObtenerDetalleAsync(string codigoCarrera, int numeroMesa, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(CARRE)   AS CodigoCarrera,
                   MESA          AS NumeroMesa,
                   TRIM(COD_MAT) AS CodigoMateria,
                   COALESCE(LLAMADO, 0) AS Llamado,
                   FECH_EXA      AS FechaExamen,
                   COALESCE(HORA, 0)  AS Hora,
                   TRIM(TITULAR) AS Titular,
                   TRIM(VOCAL1)  AS Vocal1,
                   TRIM(VOCAL2)  AS Vocal2,
                   COALESCE(AULA, 0)  AS Aula,
                   COALESCE(COMI1, 0) AS Comision1,
                   COALESCE(COMI2, 0) AS Comision2,
                   COALESCE(COMI3, 0) AS Comision3,
                   TRIM(TIPMES)  AS CodigoTipo
            FROM MESAS
            WHERE CARRE = @Carre AND MESA = @Mesa
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<MesaDetailDto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera, Mesa = numeroMesa }, cancellationToken: ct)).ConfigureAwait(false);
    }

    private static string ArmarWhere(MesasFiltro filtro, DynamicParameters parametros)
    {
        var condiciones = new List<string> { "M.CARRE = @Carre" };
        parametros.Add("Carre", filtro.CodigoCarrera);

        if (!string.IsNullOrWhiteSpace(filtro.CodigoMateria))
        {
            condiciones.Add("M.COD_MAT = @Mat");
            parametros.Add("Mat", filtro.CodigoMateria.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add("(A.SIGLA CONTAINING @Texto OR A.DESCRIPCI CONTAINING @Texto)");
            parametros.Add("Texto", filtro.Texto.Trim());
        }

        return "WHERE " + string.Join(" AND ", condiciones);
    }

    private static string ArmarOrderBy(MesasFiltro filtro)
    {
        if (filtro.OrdenarPor is not null && ColumnasOrdenables.TryGetValue(filtro.OrdenarPor, out var columna))
        {
            var direccion = filtro.OrdenDescendente ? "DESC" : "ASC";
            return $"{columna} {direccion}, M.COD_MAT";
        }

        return OrdenDefecto;
    }
}
