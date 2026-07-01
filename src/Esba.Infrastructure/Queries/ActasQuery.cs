using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Lecturas Dapper de las actas de examen. Reescritura parametrizada de los SELECT
/// concatenados de las pantallas legacy de actas.
/// </summary>
/// <remarks>
/// El legacy comparaba <c>CUA_ANIO</c> sin barra en la cabecera (COMARM) pero con
/// barra en el detalle (CURSADA), pese a que ambas columnas son CHAR(3) "124": una
/// inconsistencia latente que solo funcionaba si el usuario tipeaba sin barra. Acá se
/// normaliza quitando la barra para ambas consultas. La condición se compara con
/// <c>TRIM</c> en cabecera y detalle (el legacy no trimeaba en el EXISTS).
/// </remarks>
public sealed class ActasQuery : IActasQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public ActasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static string NormalizarCuaAnio(string cuatrimestreAnio) =>
        (cuatrimestreAnio ?? string.Empty).Replace("/", string.Empty, StringComparison.Ordinal).Trim();

    public async Task<IReadOnlyList<ActaComisionCabeceraDto>> ObtenerCabecerasComisionAsync(
        string codigoCarrera,
        string cuatrimestreAnio,
        short? cutuco,
        string? codigoMateria,
        IReadOnlyList<string> condiciones,
        bool filtrarPorCondicion,
        CancellationToken ct)
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

        if (!string.IsNullOrWhiteSpace(codigoMateria))
        {
            filtros.Add("C.COD_MAT = @CodMat");
            parametros.Add("CodMat", codigoMateria);
        }

        if (filtrarPorCondicion)
        {
            filtros.Add("""
                EXISTS(SELECT 1 FROM CURSADA CU
                       WHERE CU.CARRE = C.CARRE AND CU.COD_MAT = C.COD_MAT
                         AND CU.CUTUCO = C.CUTUCO AND CU.CUA_ANIO = C.CUA_ANIO
                         AND TRIM(CU.CONDICION) IN @Condiciones)
                """);
            parametros.Add("Condiciones", condiciones);
        }

        var sql = $"""
            SELECT C.CUTUCO            AS Cutuco,
                   TRIM(C.COD_MAT)     AS CodigoMateria,
                   TRIM(M.DESCRIPCI)   AS DescripcionMateria,
                   TRIM(D.DOCENTE)     AS Docente
            FROM COMARM C
            LEFT OUTER JOIN DOCENTES D ON C.CODPROFES = D.CODPROFES
            LEFT OUTER JOIN MATERIAS M ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY C.CUTUCO, C.COD_MAT
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<ActaComisionCabeceraDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<ActaAlumnoDto>> ObtenerAlumnosComisionAsync(
        string codigoCarrera,
        string cuatrimestreAnio,
        short? cutuco,
        string? codigoMateria,
        IReadOnlyList<string> condiciones,
        CancellationToken ct)
    {
        var parametros = new DynamicParameters();
        parametros.Add("Carre", codigoCarrera);
        parametros.Add("CuaAnio", NormalizarCuaAnio(cuatrimestreAnio));
        parametros.Add("Condiciones", condiciones);

        var filtros = new List<string>
        {
            "C.CARRE = @Carre",
            "C.CUA_ANIO = @CuaAnio",
            "TRIM(C.CONDICION) IN @Condiciones",
            "A.BAJA = 'N'",
        };
        if (cutuco.HasValue)
        {
            filtros.Add("C.CUTUCO = @Cutuco");
            parametros.Add("Cutuco", cutuco.Value);
        }

        if (!string.IsNullOrWhiteSpace(codigoMateria))
        {
            filtros.Add("C.COD_MAT = @CodMat");
            parametros.Add("CodMat", codigoMateria);
        }

        var sql = $"""
            SELECT DISTINCT
                   TRIM(A.COD_ALU)  AS CodigoAlumno,
                   TRIM(A.APELLIDO) AS Apellido,
                   TRIM(A.NOM_APE)  AS Nombre,
                   C.CUTUCO         AS Cutuco,
                   TRIM(C.COD_MAT)  AS CodigoMateria
            FROM CURSADA C
            LEFT OUTER JOIN ALUMNOS A ON C.COD_ALU = A.COD_ALU AND C.CARRE = A.CARRE
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY C.CUTUCO, A.APELLIDO, A.NOM_APE
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<ActaAlumnoDto>(
            new CommandDefinition(sql, parametros, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<ActaMesaCabeceraDto?> ObtenerCabeceraMesaAsync(int mesa, string codigoCarrera, CancellationToken ct)
    {
        const string sql = """
            SELECT COALESCE(TRIM(T.DOCENTE), '')
                   || COALESCE(' - ' || TRIM(V1.DOCENTE), '')
                   || COALESCE(' - ' || TRIM(V2.DOCENTE), '')   AS Docente,
                   M.COMI1                                       AS Cutuco,
                   TRIM(MA.DESCRIPCI)                            AS DescripcionMateria,
                   EXTRACT(DAY   FROM M.FECH_EXA)                AS Dia,
                   EXTRACT(MONTH FROM M.FECH_EXA)                AS Mes,
                   EXTRACT(YEAR  FROM M.FECH_EXA)                AS Anio,
                   MA.CUATRIM                                    AS CuatrimestreMateria
            FROM MESAS M
            LEFT OUTER JOIN DOCENTES T  ON M.TITULAR = T.CODPROFES
            LEFT OUTER JOIN DOCENTES V1 ON M.VOCAL1  = V1.CODPROFES
            LEFT OUTER JOIN DOCENTES V2 ON M.VOCAL2  = V2.CODPROFES
            LEFT OUTER JOIN MATERIAS MA ON MA.CODMATERI = M.COD_MAT AND MA.CODCARRE = M.CARRE
            WHERE M.MESA = @Mesa AND M.CARRE = @Carre
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<ActaMesaCabeceraDto>(
            new CommandDefinition(sql, new { Mesa = mesa, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ActaAlumnoDto>> ObtenerAlumnosMesaAsync(
        int mesa, string codigoCarrera, string tipoExamen, CancellationToken ct)
    {
        // PERM_EXA es VARCHAR numérico en el SP; el legacy lo lee AsInteger.
        const string sql = """
            SELECT CAST(P.PERM_EXA AS INTEGER) AS PermisoExamen,
                   TRIM(P.COD_ALU)             AS CodigoAlumno,
                   TRIM(P.APELLIDO)            AS Apellido,
                   TRIM(P.NOM_APE)             AS Nombre
            FROM XXX_MESAS_ALUMNOS(@Mesa, @Carre, @Tipo) P
            ORDER BY P.APELLIDO, P.NOM_APE
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<ActaAlumnoDto>(
            new CommandDefinition(sql, new { Mesa = mesa, Carre = codigoCarrera, Tipo = tipoExamen }, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }
}
