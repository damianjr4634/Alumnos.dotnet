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
               ORDEN           AS Orden,
               TRIM(ESTADO)    AS Estado
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

        if (filtro.DadaDeBaja is { } baja)
        {
            // ESTADO='B' es baja; cualquier otro valor (incl. NULL) es activa.
            condiciones.Add(baja ? "ESTADO = 'B'" : "(ESTADO IS NULL OR ESTADO <> 'B')");
        }

        return "WHERE " + string.Join(" AND ", condiciones);
    }

    public async Task<MateriaDetailDto?> ObtenerDetalleAsync(string codigoCarrera, string codigo, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(CODMATERI) AS Codigo,
                   TRIM(CODCARRE)  AS CodigoCarrera,
                   TRIM(DESCRIPCI) AS Nombre,
                   TRIM(SIGLA)     AS Sigla,
                   CUATRIM         AS Cuatrimestre,
                   ORDEN           AS Orden,
                   ANUAL           AS Anual,
                   PROMOCION       AS Promocion,
                   APRSFINAL       AS AprSinFinal,
                   TRIM(EQUIVALE)  AS Equivale,
                   TRIM(CORRELATIV) AS CorrelativasCursada,
                   TRIM(CORREFINAL) AS CorrelativasFinal,
                   TRIM(ESTADO)    AS Estado
            FROM MATERIAS
            WHERE CODCARRE = @Carre AND CODMATERI = @Codigo
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QueryFirstOrDefaultAsync<DetalleRow>(new CommandDefinition(
            sql, new { Carre = codigoCarrera, Codigo = codigo }, cancellationToken: ct)).ConfigureAwait(false);

        if (fila is null)
        {
            return null;
        }

        return new MateriaDetailDto
        {
            Codigo = fila.Codigo,
            CodigoCarrera = fila.CodigoCarrera,
            Nombre = fila.Nombre,
            Sigla = fila.Sigla,
            Cuatrimestre = fila.Cuatrimestre ?? 0,
            Orden = fila.Orden ?? 0,
            EsAnual = fila.Anual == "S",
            AdmitePromocion = fila.Promocion == "S",
            ApruebaSinFinal = fila.AprSinFinal == "S",
            CodigoEquivalencia = string.IsNullOrWhiteSpace(fila.Equivale) ? null : fila.Equivale,
            CorrelativasCursada = SepararCodigos(fila.CorrelativasCursada),
            CorrelativasFinal = SepararCodigos(fila.CorrelativasFinal),
            DadaDeBaja = fila.Estado == "B",
        };
    }

    /// <summary>Separa los códigos unidos por '-' (formato CORRELATIV/CORREFINAL legacy).</summary>
    private static string[] SepararCodigos(string? unidos) =>
        string.IsNullOrWhiteSpace(unidos)
            ? []
            : unidos.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>Fila cruda del detalle; los CHAR(1) llegan como 'S'/'N'/'B'.</summary>
    private sealed record DetalleRow
    {
        public string Codigo { get; init; } = string.Empty;
        public string CodigoCarrera { get; init; } = string.Empty;
        public string? Nombre { get; init; }
        public string? Sigla { get; init; }
        public short? Cuatrimestre { get; init; }
        public short? Orden { get; init; }
        public string? Anual { get; init; }
        public string? Promocion { get; init; }
        public string? AprSinFinal { get; init; }
        public string? Equivale { get; init; }
        public string? CorrelativasCursada { get; init; }
        public string? CorrelativasFinal { get; init; }
        public string? Estado { get; init; }
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
