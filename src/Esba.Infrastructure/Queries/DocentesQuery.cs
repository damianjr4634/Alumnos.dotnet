using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.Common;
using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

public sealed class DocentesQuery : IDocentesQuery
{
    private const string OrdenDefecto = "CODPROFES";

    private static readonly Dictionary<string, string> ColumnasOrdenables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Codigo"] = "CODPROFES",
            ["Nombre"] = "DOCENTE",
            ["Localidad"] = "LOCALIDAD",
        };

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

    public async Task<PagedResult<DocenteListItemDto>> BuscarAsync(DocentesFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        var parametros = new DynamicParameters();
        var where = ArmarWhere(filtro, parametros);
        var orderBy = ArmarOrderBy(filtro);

        var sqlItems = $"""
            SELECT TRIM(CODPROFES) AS Codigo,
                   TRIM(DOCENTE)   AS Nombre,
                   TRIM(NRODOCUM)  AS NumeroDocumento,
                   TRIM(LOCALIDAD) AS Localidad,
                   FECHA_BAJ       AS FechaBaja
            FROM DOCENTES
            {where}
            ORDER BY {orderBy}
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        parametros.Add("Skip", filtro.Skip);
        parametros.Add("Take", filtro.Take);

        var sqlTotal = $"SELECT COUNT(*) FROM DOCENTES {where}";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var items = await connection.QueryAsync<DocenteListItemDto>(
            new CommandDefinition(sqlItems, parametros, cancellationToken: ct)).ConfigureAwait(false);
        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sqlTotal, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new PagedResult<DocenteListItemDto> { Items = items.AsList(), Total = total };
    }

    public async Task<DocenteDetailDto?> ObtenerDetalleAsync(string codigo, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(CODPROFES) AS Codigo,
                   TRIM(DOCENTE)   AS Nombre,
                   TRIM(TIPODOC)   AS TipoDocumento,
                   TRIM(NRODOCUM)  AS NumeroDocumento,
                   FEC_NAC         AS FechaNacimiento,
                   TRIM(DI_ECCION) AS Direccion,
                   TRIM(PISO)      AS Piso,
                   TRIM(DEPTO)     AS Departamento,
                   TRIM(COD_POST)  AS CodigoPostal,
                   TRIM(LOCALIDAD) AS Localidad,
                   TRIM(TELEFONO_P) AS TelefonoParticular,
                   TRIM(TELEFONO_M) AS TelefonoMensajes,
                   TRIM(INTERNO)   AS Interno,
                   FECHA_ING       AS FechaIngreso,
                   FECHA_BAJ       AS FechaBaja,
                   CASE WHEN LICENCIA = 'S' THEN 1 ELSE 0 END AS EnLicencia,
                   LICENFECH       AS FechaLicencia
            FROM DOCENTES
            WHERE CODPROFES = @Codigo
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<DocenteDetailDto>(new CommandDefinition(
            sql, new { Codigo = codigo }, cancellationToken: ct)).ConfigureAwait(false);
    }

    private static string ArmarWhere(DocentesFiltro filtro, DynamicParameters parametros)
    {
        var condiciones = new List<string>();

        if (!filtro.IncluirBajas)
        {
            condiciones.Add("FECHA_BAJ IS NULL");
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add("(CODPROFES CONTAINING @Texto OR DOCENTE CONTAINING @Texto"
                + " OR NRODOCUM CONTAINING @Texto OR LOCALIDAD CONTAINING @Texto)");
            parametros.Add("Texto", filtro.Texto.Trim());
        }

        return condiciones.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", condiciones);
    }

    private static string ArmarOrderBy(DocentesFiltro filtro)
    {
        if (filtro.OrdenarPor is not null && ColumnasOrdenables.TryGetValue(filtro.OrdenarPor, out var columna))
        {
            var direccion = filtro.Descendente ? "DESC" : "ASC";
            return $"{columna} {direccion}, CODPROFES";
        }

        return OrdenDefecto;
    }
}
