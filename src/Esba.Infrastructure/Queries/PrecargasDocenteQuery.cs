using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.Common;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Enums;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Listados de precargas de docentes para secretaría (DOC_CARGA_COMISION / DOC_CARGA_MESA
/// con sus joins descriptivos). Orden por defecto: finalizadas primero (las que esperan
/// revisión), después borradores, después efectivizadas; dentro de cada grupo la más
/// reciente arriba.
/// </summary>
public sealed class PrecargasDocenteQuery : IPrecargasDocenteQuery
{
    private const string OrdenEstado = "CASE TRIM(DC.ESTADO) WHEN 'FIN' THEN 0 WHEN 'BOR' THEN 1 ELSE 2 END";

    private readonly FbConnectionFactory _connectionFactory;

    public PrecargasDocenteQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PagedResult<PrecargaComisionListItemDto>> BuscarComisionesAsync(PrecargasDocenteFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        const string select = """
            SELECT DC.ID              AS CargaId,
                   TRIM(DC.CARRE)     AS CodigoCarrera,
                   TRIM(K.DESCORT)    AS NombreCarrera,
                   DC.CUTUCO          AS Cutuco,
                   TRIM(DC.COD_MAT)   AS CodigoMateria,
                   TRIM(M.SIGLA)      AS SiglaMateria,
                   TRIM(DC.CUA_ANIO)  AS CuatrimestreAnio,
                   TRIM(DC.CODPROFES) AS CodigoDocente,
                   TRIM(D.DOCENTE)    AS NombreDocente,
                   TRIM(DC.ESTADO)    AS EstadoCodigo,
                   DC.FEC_MODIF       AS FechaModificacion,
                   DC.FEC_FINAL       AS FechaFinalizacion,
                   DC.FEC_EFECTIVO    AS FechaEfectivizacion,
                   (SELECT COUNT(*) FROM DOC_CARGA_COMISION_DET X WHERE X.CARGA_ID = DC.ID) AS CantidadAlumnos
            """;
        const string from = """
            FROM DOC_CARGA_COMISION DC
            LEFT OUTER JOIN CARRERA K ON K.CARRE = DC.CARRE
            LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = DC.COD_MAT AND M.CODCARRE = DC.CARRE
            LEFT OUTER JOIN DOCENTES D ON D.CODPROFES = DC.CODPROFES
            """;
        var ordenables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CuatrimestreAnio"] = "DC.CUA_ANIO",
            ["NombreDocente"] = "D.DOCENTE",
            ["SiglaMateria"] = "M.SIGLA",
            ["FechaFinalizacion"] = "DC.FEC_FINAL",
        };

        return await BuscarAsync<PrecargaComisionListItemDto>(select, from, filtro, ordenables,
            textoSobre: "(D.DOCENTE CONTAINING @Texto OR M.SIGLA CONTAINING @Texto OR M.DESCRIPCI CONTAINING @Texto)",
            ordenDefecto: $"{OrdenEstado}, DC.FEC_FINAL DESC, DC.FEC_MODIF DESC", ct).ConfigureAwait(false);
    }

    public async Task<PagedResult<PrecargaMesaListItemDto>> BuscarMesasAsync(PrecargasDocenteFiltro filtro, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filtro);

        const string select = """
            SELECT DC.ID              AS CargaId,
                   TRIM(DC.CARRE)     AS CodigoCarrera,
                   TRIM(K.DESCORT)    AS NombreCarrera,
                   DC.MESA            AS NumeroMesa,
                   TRIM(MS.COD_MAT)   AS CodigoMateria,
                   TRIM(M.SIGLA)      AS SiglaMateria,
                   MS.FECH_EXA        AS FechaExamen,
                   TRIM(DC.CODPROFES) AS CodigoDocente,
                   TRIM(D.DOCENTE)    AS NombreDocente,
                   TRIM(DC.ESTADO)    AS EstadoCodigo,
                   DC.FEC_MODIF       AS FechaModificacion,
                   DC.FEC_FINAL       AS FechaFinalizacion,
                   DC.FEC_EFECTIVO    AS FechaEfectivizacion,
                   (SELECT COUNT(*) FROM DOC_CARGA_MESA_DET X WHERE X.CARGA_ID = DC.ID) AS CantidadAlumnos
            """;
        const string from = """
            FROM DOC_CARGA_MESA DC
            LEFT OUTER JOIN CARRERA K ON K.CARRE = DC.CARRE
            LEFT OUTER JOIN MESAS MS ON MS.CARRE = DC.CARRE AND MS.MESA = DC.MESA
            LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = MS.COD_MAT AND M.CODCARRE = MS.CARRE
            LEFT OUTER JOIN DOCENTES D ON D.CODPROFES = DC.CODPROFES
            """;
        var ordenables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FechaExamen"] = "MS.FECH_EXA",
            ["NombreDocente"] = "D.DOCENTE",
            ["SiglaMateria"] = "M.SIGLA",
            ["FechaFinalizacion"] = "DC.FEC_FINAL",
        };

        return await BuscarAsync<PrecargaMesaListItemDto>(select, from, filtro, ordenables,
            textoSobre: "(D.DOCENTE CONTAINING @Texto OR M.SIGLA CONTAINING @Texto OR M.DESCRIPCI CONTAINING @Texto)",
            ordenDefecto: $"{OrdenEstado}, DC.FEC_FINAL DESC, DC.FEC_MODIF DESC", ct).ConfigureAwait(false);
    }

    private async Task<PagedResult<T>> BuscarAsync<T>(
        string select, string from, PrecargasDocenteFiltro filtro, Dictionary<string, string> ordenables,
        string textoSobre, string ordenDefecto, CancellationToken ct)
    {
        var parametros = new DynamicParameters();
        var condiciones = new List<string>();

        if (filtro.CarrerasPermitidas is not null)
        {
            condiciones.Add("TRIM(DC.CARRE) IN @Carreras");
            parametros.Add("Carreras", filtro.CarrerasPermitidas.Select(c => c.Trim()).ToArray());
        }

        if (!string.IsNullOrWhiteSpace(filtro.CodigoCarrera))
        {
            condiciones.Add("DC.CARRE = @Carre");
            parametros.Add("Carre", filtro.CodigoCarrera.Trim());
        }

        if (filtro.Estado is { } estado)
        {
            condiciones.Add("DC.ESTADO = @Estado");
            parametros.Add("Estado", EstadoCargaDocenteCodigo.ACodigo(estado));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            condiciones.Add(textoSobre);
            parametros.Add("Texto", filtro.Texto.Trim());
        }

        var where = condiciones.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", condiciones);
        var orderBy = filtro.OrdenarPor is not null && ordenables.TryGetValue(filtro.OrdenarPor, out var columna)
            ? $"{columna} {(filtro.Descendente ? "DESC" : "ASC")}, DC.ID"
            : ordenDefecto;

        var sqlItems = $"""
            {select}
            {from}
            {where}
            ORDER BY {orderBy}
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        parametros.Add("Skip", filtro.Skip);
        parametros.Add("Take", filtro.Take);
        var sqlTotal = $"SELECT COUNT(*) {from} {where}";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var items = await connection.QueryAsync<T>(new CommandDefinition(sqlItems, parametros, cancellationToken: ct)).ConfigureAwait(false);
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sqlTotal, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new PagedResult<T> { Items = items.AsList(), Total = total };
    }
}
