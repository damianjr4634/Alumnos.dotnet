using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Ministerio;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Lecturas Dapper del padrón de comisiones al Ministerio. Reescritura parametrizada de
/// los SELECT concatenados de ComisionesAlMinisterio.pas: SqlComi de ImprimirClick
/// (CUTUCO distintos de COMARM), SqlDatos de ImprimirClick (nómina) y SqlComi de
/// BitBtn1Click (padrón Excel con CARRERA y TUTORES). Los dos últimos se unifican en
/// una sola consulta cuyas columnas son el superconjunto; la codificación
/// (IIF/CASE del legacy) se hace en el dominio.
/// </summary>
/// <remarks>
/// Como en las carpetas por comisión, el legacy comparaba <c>CUA_ANIO</c> a veces con
/// barra y a veces sin (ImprimirClick la quitaba para COMARM pero no para CURSADA);
/// acá se normaliza siempre al formato físico "124". Otra diferencia deliberada: los
/// subselects de TUTORES del legacy eran escalares sin FIRST 1 y fallaban en Firebird
/// si un alumno tenía cargados padre y madre (más de una fila); se toma el primero por
/// FPARENT como el ORDER BY original lo insinuaba.
/// </remarks>
public sealed class PadronMinisterioQuery : IPadronMinisterioQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public PadronMinisterioQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static string NormalizarCuaAnio(string cuatrimestreAnio) =>
        (cuatrimestreAnio ?? string.Empty).Replace("/", string.Empty, StringComparison.Ordinal).Trim();

    public async Task<CarreraMinisterioDto?> ObtenerCarreraAsync(string codigoCarrera, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(CARRE)      AS Codigo,
                   TRIM(DESCARRE)   AS Nombre,
                   TRIM(TIPO)       AS Tipo,
                   TRIM(RESOLUCION) AS Resolucion,
                   IIF(DISTANCIA = 'S', TRUE, FALSE) AS EsADistancia
            FROM CARRERA
            WHERE TRIM(CARRE) = @Carre
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<CarreraMinisterioDto>(
            new CommandDefinition(sql, new { Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<short>> ObtenerComisionesAsync(
        string codigoCarrera, string cuatrimestreAnio, short? cutuco, CancellationToken ct)
    {
        var parametros = new DynamicParameters();
        parametros.Add("Carre", codigoCarrera);
        parametros.Add("CuaAnio", NormalizarCuaAnio(cuatrimestreAnio));

        var filtros = new List<string> { "C.CARRE = @Carre", "C.CUA_ANIO = @CuaAnio" };
        if (cutuco.HasValue)
        {
            filtros.Add("C.CUTUCO = @Cutuco");
            parametros.Add("Cutuco", cutuco.Value);
        }

        var sql = $"""
            SELECT DISTINCT C.CUTUCO
            FROM COMARM C
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY 1
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<short>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<PadronMinisterioAlumnoDto>> ObtenerAlumnosAsync(
        string codigoCarrera, string cuatrimestreAnio, short? cutuco, CancellationToken ct)
    {
        var parametros = new DynamicParameters();
        parametros.Add("Carre", codigoCarrera);
        parametros.Add("CuaAnio", NormalizarCuaAnio(cuatrimestreAnio));

        var filtros = new List<string>
        {
            "C.CARRE = @Carre",
            "C.CUA_ANIO = @CuaAnio",
            "TRIM(C.CONDICION) IN ('CURSANDO', 'RECURSANDO')",
            "A.BAJA = 'N'",
        };
        if (cutuco.HasValue)
        {
            filtros.Add("C.CUTUCO = @Cutuco");
            parametros.Add("Cutuco", cutuco.Value);
        }

        // Mismo ORDER BY que el legacy: la condición ordena CURSANDO antes que
        // RECURSANDO, que es lo que separaba los dos bloques de la nómina.
        var sql = $"""
            SELECT DISTINCT
                   C.CUTUCO          AS Cutuco,
                   A.COD_ALU         AS CodigoAlumno,
                   TRIM(A.APELLIDO)  AS Apellido,
                   TRIM(A.NOM_APE)   AS Nombre,
                   TRIM(C.CONDICION) AS Condicion,
                   TRIM(A.SEXO)      AS Sexo,
                   A.FEC_NAC         AS FechaNacimiento,
                   TRIM(A.NACIONAL)  AS Nacionalidad,
                   TRIM(A.LUG_NAC)   AS LugarNacimiento,
                   TRIM(A.DOMI)      AS Domicilio,
                   A.COD_POS         AS CodigoPostal,
                   TRIM(A.LOCALI)    AS Localidad,
                   TRIM(A.CSECU)     AS ColegioSecundario,
                   TRIM(A.TSECU)     AS TituloSecundario,
                   (SELECT FIRST 1 TRIM(T.FAPELLIDO) FROM TUTORES T
                     WHERE T.COD_ALU = C.COD_ALU AND T.CARRE = C.CARRE
                       AND UPPER(T.FPARENT) IN ('PADRE', 'MADRE', 'TUTOR')
                     ORDER BY T.FPARENT) AS ApellidoTutor,
                   (SELECT FIRST 1 TRIM(T.FNOMBRE) FROM TUTORES T
                     WHERE T.COD_ALU = C.COD_ALU AND T.CARRE = C.CARRE
                       AND UPPER(T.FPARENT) IN ('PADRE', 'MADRE', 'TUTOR')
                     ORDER BY T.FPARENT) AS NombreTutor,
                   (SELECT FIRST 1 T.FDNI FROM TUTORES T
                     WHERE T.COD_ALU = C.COD_ALU AND T.CARRE = C.CARRE
                       AND UPPER(T.FPARENT) IN ('PADRE', 'MADRE', 'TUTOR')
                     ORDER BY T.FPARENT) AS DniTutor
            FROM CURSADA C
            LEFT OUTER JOIN ALUMNOS A ON C.COD_ALU = A.COD_ALU AND C.CARRE = A.CARRE
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY C.CUTUCO, TRIM(C.CONDICION), TRIM(A.APELLIDO), TRIM(A.NOM_APE)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<PadronMinisterioAlumnoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }
}
