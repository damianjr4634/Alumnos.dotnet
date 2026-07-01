using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Lecturas Dapper de la regularización. Reescritura parametrizada del INSERT ... SELECT
/// que las pantallas legacy volcaban a "$$$CURSADA" (RegularizacionDeMaterias_nuevo.pas
/// líneas 502-516; XComision líneas 1623-1638). Sin staging: se lee CURSADA directo y el
/// estado de edición vive en el componente.
/// </summary>
/// <remarks>
/// CUA_ANIO es CHAR(3) "124": se normaliza quitando la barra que el operador pueda tipear.
/// El autocompletado de faltas desde XXX_CONT_FALTAS (asistencias, hito 7) queda diferido:
/// las faltas se leen de CURSADA y el usuario las edita. // TODO-migrar prefill de faltas.
/// </remarks>
public sealed class RegularizacionQuery : IRegularizacionQuery
{
    private const string SelectBase = """
        SELECT TRIM(C.COD_ALU)   AS CodigoAlumno,
               TRIM(A.APELLIDO)  AS Apellido,
               TRIM(A.NOM_APE)   AS Nombre,
               TRIM(C.COD_MAT)   AS CodigoMateria,
               TRIM(M.SIGLA)     AS SiglaMateria,
               C.CUTUCO          AS Cutuco,
               TRIM(C.CUA_ANIO)  AS CuatrimestreAnio,
               TRIM(C.CONDICION) AS Condicion,
               C.TP_EVA          AS TpEva,
               C.TP_EVA2         AS TpEva2,
               C.RECUP           AS Recuperatorio,
               C.TOT_HORAS       AS TotalHoras,
               C.INASIST         AS Inasistencias,
               C.JUSTIF          AS Justificadas,
               IIF(TRIM(COALESCE(M.PROMOCION, 'N')) = 'S', TRUE, FALSE) AS MateriaPromociona,
               IIF(TRIM(COALESCE(M.APRSFINAL, 'N')) = 'S', TRUE, FALSE) AS MateriaApruebaSinFinal
        FROM CURSADA C
        LEFT OUTER JOIN ALUMNOS A ON C.COD_ALU = A.COD_ALU AND C.CARRE = A.CARRE
        LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = C.COD_MAT AND M.CODCARRE = C.CARRE
        """;

    // Variante bachillerato: agrega la nota "a regular" (REGULAR), la nota definitiva
    // (FINAL1), la fecha (FECHA1) y el flag EnRecursa (EXISTS en RECURSA, para el rescate
    // a RECURSANDO de _BAC).
    private const string SelectBaseBachillerato = """
        SELECT TRIM(C.COD_ALU)   AS CodigoAlumno,
               TRIM(A.APELLIDO)  AS Apellido,
               TRIM(A.NOM_APE)   AS Nombre,
               TRIM(C.COD_MAT)   AS CodigoMateria,
               TRIM(M.SIGLA)     AS SiglaMateria,
               C.CUTUCO          AS Cutuco,
               TRIM(C.CUA_ANIO)  AS CuatrimestreAnio,
               TRIM(C.CONDICION) AS Condicion,
               C.TP_EVA          AS TpEva,
               C.TP_EVA2         AS TpEva2,
               C.RECUP           AS Recuperatorio,
               C.REGULAR         AS NotaRegular,
               C.TOT_HORAS       AS TotalHoras,
               C.INASIST         AS Inasistencias,
               C.JUSTIF          AS Justificadas,
               C.FINAL1          AS NotaDefinitiva,
               C.FECHA1          AS Fecha,
               IIF(EXISTS(SELECT 1 FROM RECURSA R
                          WHERE R.COD_ALU = C.COD_ALU AND R.CARRE = C.CARRE AND R.CUTUCO = C.CUTUCO
                            AND R.COD_MAT = C.COD_MAT AND R.CUA_ANIO = C.CUA_ANIO), TRUE, FALSE) AS EnRecursa
        FROM CURSADA C
        LEFT OUTER JOIN ALUMNOS A ON C.COD_ALU = A.COD_ALU AND C.CARRE = A.CARRE
        LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = C.COD_MAT AND M.CODCARRE = C.CARRE
        """;

    // Variante secundario (333/650): 3 trimestres con sus fechas + exámenes de diciembre y marzo.
    private const string SelectBase333 = """
        SELECT TRIM(C.COD_ALU)   AS CodigoAlumno,
               TRIM(A.APELLIDO)  AS Apellido,
               TRIM(A.NOM_APE)   AS Nombre,
               TRIM(C.COD_MAT)   AS CodigoMateria,
               TRIM(M.SIGLA)     AS SiglaMateria,
               C.CUTUCO          AS Cutuco,
               TRIM(C.CUA_ANIO)  AS CuatrimestreAnio,
               TRIM(C.CONDICION) AS Condicion,
               C.TP_EVA          AS TpEva,
               C.TP_EVA2         AS TpEva2,
               C.TP_EVA3         AS TpEva3,
               C.FEC_EVA1        AS FecEva1,
               C.FEC_EVA2        AS FecEva2,
               C.FEC_EVA3        AS FecEva3,
               C.NOTADIC         AS NotaDic,
               C.FECHDIC         AS FechDic,
               C.NOTAMAR         AS NotaMar,
               C.FECHMAR         AS FechMar,
               C.TOT_HORAS       AS TotalHoras,
               C.INASIST         AS Inasistencias,
               C.JUSTIF          AS Justificadas,
               C.FECHA1          AS Fecha
        FROM CURSADA C
        LEFT OUTER JOIN ALUMNOS A ON C.COD_ALU = A.COD_ALU AND C.CARRE = A.CARRE
        LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = C.COD_MAT AND M.CODCARRE = C.CARRE
        """;

    private readonly FbConnectionFactory _connectionFactory;

    public RegularizacionQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private static string NormalizarCuaAnio(string cuatrimestreAnio) =>
        (cuatrimestreAnio ?? string.Empty).Replace("/", string.Empty, StringComparison.Ordinal).Trim();

    public async Task<IReadOnlyList<RegularizacionCursadaDto>> ObtenerPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct)
    {
        var sql = SelectBase + """

            WHERE C.CARRE = @Carre AND C.COD_ALU = @CodAlu
            ORDER BY C.CUA_ANIO DESC, C.CONDICION, C.COD_MAT
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<RegularizacionCursadaDto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera, CodAlu = codigoAlumno }, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<RegularizacionCursadaDto>> ObtenerPorComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoMateria, CancellationToken ct)
    {
        var sql = SelectBase + """

            WHERE C.CUTUCO = @Cutuco AND C.COD_MAT = @CodMat AND C.CUA_ANIO = @CuaAnio
              AND C.CARRE = @Carre AND TRIM(C.CONDICION) <> 'REGULAR' AND A.BAJA = 'N'
            ORDER BY C.CONDICION, A.APELLIDO, A.NOM_APE
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<RegularizacionCursadaDto>(new CommandDefinition(
            sql,
            new
            {
                Carre = codigoCarrera,
                Cutuco = cutuco,
                CuaAnio = NormalizarCuaAnio(cuatrimestreAnio),
                CodMat = codigoMateria,
            },
            cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<RegularizacionBachilleratoDto>> ObtenerBachilleratoPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct)
    {
        var sql = SelectBaseBachillerato + """

            WHERE C.CARRE = @Carre AND C.COD_ALU = @CodAlu
            ORDER BY C.CUA_ANIO DESC, C.CONDICION, C.COD_MAT
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<RegularizacionBachilleratoDto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera, CodAlu = codigoAlumno }, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<RegularizacionBachilleratoDto>> ObtenerBachilleratoPorComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoMateria, CancellationToken ct)
    {
        var sql = SelectBaseBachillerato + """

            WHERE C.CUTUCO = @Cutuco AND C.COD_MAT = @CodMat AND C.CUA_ANIO = @CuaAnio
              AND C.CARRE = @Carre AND TRIM(C.CONDICION) <> 'REGULAR' AND A.BAJA = 'N'
            ORDER BY C.CONDICION, A.APELLIDO, A.NOM_APE
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<RegularizacionBachilleratoDto>(new CommandDefinition(
            sql,
            new
            {
                Carre = codigoCarrera,
                Cutuco = cutuco,
                CuaAnio = NormalizarCuaAnio(cuatrimestreAnio),
                CodMat = codigoMateria,
            },
            cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<Regularizacion333Dto>> Obtener333PorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct)
    {
        var sql = SelectBase333 + """

            WHERE C.CARRE = @Carre AND C.COD_ALU = @CodAlu
            ORDER BY C.CUA_ANIO DESC, C.CONDICION, C.COD_MAT
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<Regularizacion333Dto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera, CodAlu = codigoAlumno }, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<Regularizacion333Dto>> Obtener333PorComisionAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoMateria, CancellationToken ct)
    {
        var sql = SelectBase333 + """

            WHERE C.CUTUCO = @Cutuco AND C.COD_MAT = @CodMat AND C.CUA_ANIO = @CuaAnio
              AND C.CARRE = @Carre AND TRIM(C.CONDICION) <> 'REGULAR' AND A.BAJA = 'N'
            ORDER BY C.CONDICION, A.APELLIDO, A.NOM_APE
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<Regularizacion333Dto>(new CommandDefinition(
            sql,
            new
            {
                Carre = codigoCarrera,
                Cutuco = cutuco,
                CuaAnio = NormalizarCuaAnio(cuatrimestreAnio),
                CodMat = codigoMateria,
            },
            cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }
}
