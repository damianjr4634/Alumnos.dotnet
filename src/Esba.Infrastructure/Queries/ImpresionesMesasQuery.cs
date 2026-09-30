using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Lecturas Dapper de las impresiones de mesas. Reescritura parametrizada de los SELECT
/// concatenados de Impresiones.pas: SqlDatos/SqlDatos2 de Imp_Mesas_citacion y SqlDatos
/// de Imp_Mesas_ParteDiario.
/// </summary>
/// <remarks>
/// Diferencias deliberadas con el legacy: (1) los docentes citados se obtienen con JOIN
/// en vez de LEFT JOIN — el LEFT producía una fila con docente NULL para las mesas sin
/// tribunal cargado, que imprimía una carta vacía "Señor Profesor:"; (2) las mesas de la
/// citación se traen para todos los docentes en una consulta (el legacy iteraba una por
/// docente) y se ordenan por HORA numérica, no por su texto ("9:30" quedaba después de
/// "18:30"); (3) la materia no se trunca a 30 caracteres (era por el ancho fijo de la
/// hoja); (4) el parte diario trae COMI2 (el legacy repetía COMI3) y los tres nombres del
/// tribunal por separado, sin el truncado a 15 caracteres.
/// </remarks>
public sealed class ImpresionesMesasQuery : IImpresionesMesasQuery
{
    private const string MateriaSql = "COALESCE(NULLIF(TRIM(M.SIGLA), ''), TRIM(M.DESCRIPCI))";

    private readonly FbConnectionFactory _connectionFactory;

    public ImpresionesMesasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static (List<string> Filtros, DynamicParameters Parametros) FiltrosCitacion(
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        string? codigoProfesorDesde,
        string? codigoProfesorHasta,
        IReadOnlyList<string> codigosCarrera)
    {
        var parametros = new DynamicParameters();
        parametros.Add("Desde", fechaDesde);
        parametros.Add("Hasta", fechaHasta);
        var filtros = new List<string> { "S.FECH_EXA BETWEEN @Desde AND @Hasta" };

        // Como el legacy, el rango de docentes solo aplica con ambos extremos.
        if (!string.IsNullOrWhiteSpace(codigoProfesorDesde) && !string.IsNullOrWhiteSpace(codigoProfesorHasta))
        {
            filtros.Add("D.CODPROFES BETWEEN @ProfDesde AND @ProfHasta");
            parametros.Add("ProfDesde", codigoProfesorDesde.Trim());
            parametros.Add("ProfHasta", codigoProfesorHasta.Trim());
        }

        var carreras = codigosCarrera.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToList();
        if (carreras.Count > 0)
        {
            filtros.Add("TRIM(S.CARRE) IN @Carreras");
            parametros.Add("Carreras", carreras);
        }

        return (filtros, parametros);
    }

    public async Task<IReadOnlyList<DocenteCitadoDto>> ObtenerDocentesCitadosAsync(
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        string? codigoProfesorDesde,
        string? codigoProfesorHasta,
        IReadOnlyList<string> codigosCarrera,
        CancellationToken ct)
    {
        var (filtros, parametros) = FiltrosCitacion(fechaDesde, fechaHasta, codigoProfesorDesde, codigoProfesorHasta, codigosCarrera);

        var sql = $"""
            SELECT DISTINCT TRIM(D.CODPROFES) AS CodigoProfesor,
                            TRIM(D.DOCENTE)   AS Docente
            FROM MESAS S
            JOIN DOCENTES D ON D.CODPROFES = S.TITULAR OR D.CODPROFES = S.VOCAL1 OR D.CODPROFES = S.VOCAL2
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY 2, 1
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<DocenteCitadoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<MesaCitacionDto>> ObtenerMesasCitacionAsync(
        DateOnly fechaDesde,
        DateOnly fechaHasta,
        string? codigoProfesorDesde,
        string? codigoProfesorHasta,
        IReadOnlyList<string> codigosCarrera,
        CancellationToken ct)
    {
        var (filtros, parametros) = FiltrosCitacion(fechaDesde, fechaHasta, codigoProfesorDesde, codigoProfesorHasta, codigosCarrera);

        var sql = $"""
            SELECT TRIM(D.CODPROFES) AS CodigoProfesor,
                   S.FECH_EXA        AS FechaExamen,
                   S.HORA            AS Hora,
                   {MateriaSql}      AS Materia,
                   S.MESA            AS Mesa,
                   S.AULA            AS Aula,
                   TRIM(S.CARRE)     AS CodigoCarrera
            FROM MESAS S
            JOIN DOCENTES D ON D.CODPROFES = S.TITULAR OR D.CODPROFES = S.VOCAL1 OR D.CODPROFES = S.VOCAL2
            LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = S.COD_MAT AND M.CODCARRE = S.CARRE
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY D.CODPROFES, S.FECH_EXA, S.HORA, {MateriaSql}, S.CARRE, S.MESA
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<MesaCitacionDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<ParteDiarioMesaDto>> ObtenerParteDiarioAsync(
        DateOnly fechaDesde, DateOnly fechaHasta, string? codigoCarrera, CancellationToken ct)
    {
        var parametros = new DynamicParameters();
        parametros.Add("Desde", fechaDesde);
        parametros.Add("Hasta", fechaHasta);
        var filtros = new List<string> { "S.FECH_EXA BETWEEN @Desde AND @Hasta" };
        if (!string.IsNullOrWhiteSpace(codigoCarrera))
        {
            filtros.Add("TRIM(S.CARRE) = @Carre");
            parametros.Add("Carre", codigoCarrera.Trim());
        }

        var sql = $"""
            SELECT S.FECH_EXA        AS FechaExamen,
                   TRIM(S.CARRE)     AS CodigoCarrera,
                   TRIM(R.DESCARRE)  AS NombreCarrera,
                   S.MESA            AS Mesa,
                   S.HORA            AS Hora,
                   {MateriaSql}      AS Materia,
                   TRIM(D1.DOCENTE)  AS Titular,
                   TRIM(D2.DOCENTE)  AS Vocal1,
                   TRIM(D3.DOCENTE)  AS Vocal2,
                   S.COMI1           AS Comision1,
                   S.COMI2           AS Comision2,
                   S.COMI3           AS Comision3,
                   (SELECT COUNT(*) FROM PERMEXA P
                     WHERE P.CARRE = S.CARRE AND P.MESA = S.MESA AND P.FECH_EXA = S.FECH_EXA) AS CantidadAlumnos,
                   S.AULA            AS Aula
            FROM MESAS S
            LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = S.COD_MAT AND M.CODCARRE = S.CARRE
            LEFT OUTER JOIN CARRERA R ON R.CARRE = S.CARRE
            LEFT OUTER JOIN DOCENTES D1 ON D1.CODPROFES = S.TITULAR
            LEFT OUTER JOIN DOCENTES D2 ON D2.CODPROFES = S.VOCAL1
            LEFT OUTER JOIN DOCENTES D3 ON D3.CODPROFES = S.VOCAL2
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY S.FECH_EXA, S.CARRE, S.HORA, S.MESA
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<ParteDiarioMesaDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<AutoridadesCarreraDto?> ObtenerAutoridadesAsync(string codigoCarrera, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(RECTOR)     AS Rector,
                   TRIM(SECRETARIA) AS Secretaria,
                   TRIM(DIRESTU)    AS DirectorEstudios
            FROM CARRERA
            WHERE TRIM(CARRE) = @Carre
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<AutoridadesCarreraDto>(
            new CommandDefinition(sql, new { Carre = codigoCarrera.Trim() }, cancellationToken: ct)).ConfigureAwait(false);
    }
}
