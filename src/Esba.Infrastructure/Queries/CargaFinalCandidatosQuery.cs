using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Candidatos a cargar nota de final en una mesa. Reescritura parametrizada del
/// SELECT de FinalesxMesayComision.BuscarMesaClick: usa el SP XXX_MESAS_ALUMNOS
/// como fuente (filtrado por mesa/carrera/tipo de examen) y lo joinea con CURSADA
/// para traer las notas/condición actuales y con ALUMNOS/MATERIAS para nombre y
/// sigla. NUMFIN (1..4) sale del mismo CASE que el legacy. Sin staging.
///
/// El SP aparece como tabla en el FROM (no como wrapper 2.B independiente) porque
/// el dato útil sale recién al joinearlo con CURSADA — igual que en el legacy.
/// </summary>
public sealed class CargaFinalCandidatosQuery : ICargaFinalCandidatosQuery
{
    private const string Sql = """
        SELECT TRIM(P.COD_ALU)   AS CodigoAlumno,
               TRIM(P.CARRE)     AS CodigoCarrera,
               TRIM(P.CODMAT)    AS CodigoMateria,
               P.MESA            AS Mesa,
               TRIM(A.APELLIDO)  AS Apellido,
               TRIM(A.NOM_APE)   AS Nombre,
               TRIM(M.SIGLA)     AS SiglaMateria,
               TRIM(C.CONDICION) AS Condicion,
               CASE WHEN COALESCE(C.FINAL1, 0) = 0 THEN 1
                    WHEN COALESCE(C.FINAL1, 0) <> 0 AND COALESCE(C.FINAL2, 0) = 0 THEN 2
                    WHEN COALESCE(C.FINAL1, 0) <> 0 AND COALESCE(C.FINAL2, 0) <> 0 AND COALESCE(C.FINAL3, 0) = 0 THEN 3
                    ELSE 4 END   AS NumeroFinal,
               C.FINAL1          AS NotaFinal1,
               C.FECHA1          AS FechaFinal1,
               C.FINAL2          AS NotaFinal2,
               C.FECHA2          AS FechaFinal2,
               C.FINAL3          AS NotaFinal3,
               C.FECHA3          AS FechaFinal3,
               TRIM(C.FACTFIN1)  AS ActaFinal1,
               TRIM(C.FACTFIN2)  AS ActaFinal2,
               TRIM(C.FACTFIN3)  AS ActaFinal3
        FROM XXX_MESAS_ALUMNOS(@Mesa, @Carre, @Tipo) P
        INNER JOIN CURSADA C ON C.COD_ALU = P.COD_ALU AND C.COD_MAT = P.CODMAT AND C.CARRE = P.CARRE
        LEFT OUTER JOIN ALUMNOS A ON A.COD_ALU = P.COD_ALU AND A.CARRE = P.CARRE
        LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = P.CODMAT AND M.CODCARRE = P.CARRE
        ORDER BY A.APELLIDO, A.NOM_APE
        """;

    // Variante por alumno (NotasExamenFinal.FormCreate): los permisos de UN alumno,
    // de cualquier mesa, directamente desde PERMEXA (nota: columna COD_MAT, no CODMAT).
    private const string SqlPorAlumno = """
        SELECT TRIM(P.COD_ALU)   AS CodigoAlumno,
               TRIM(P.CARRE)     AS CodigoCarrera,
               TRIM(P.COD_MAT)   AS CodigoMateria,
               P.MESA            AS Mesa,
               TRIM(A.APELLIDO)  AS Apellido,
               TRIM(A.NOM_APE)   AS Nombre,
               TRIM(M.SIGLA)     AS SiglaMateria,
               TRIM(C.CONDICION) AS Condicion,
               CASE WHEN COALESCE(C.FINAL1, 0) = 0 THEN 1
                    WHEN COALESCE(C.FINAL1, 0) <> 0 AND COALESCE(C.FINAL2, 0) = 0 THEN 2
                    WHEN COALESCE(C.FINAL1, 0) <> 0 AND COALESCE(C.FINAL2, 0) <> 0 AND COALESCE(C.FINAL3, 0) = 0 THEN 3
                    ELSE 4 END   AS NumeroFinal,
               C.FINAL1          AS NotaFinal1,
               C.FECHA1          AS FechaFinal1,
               C.FINAL2          AS NotaFinal2,
               C.FECHA2          AS FechaFinal2,
               C.FINAL3          AS NotaFinal3,
               C.FECHA3          AS FechaFinal3,
               TRIM(C.FACTFIN1)  AS ActaFinal1,
               TRIM(C.FACTFIN2)  AS ActaFinal2,
               TRIM(C.FACTFIN3)  AS ActaFinal3
        FROM PERMEXA P
        INNER JOIN CURSADA C ON C.COD_ALU = P.COD_ALU AND C.COD_MAT = P.COD_MAT AND C.CARRE = P.CARRE
        LEFT OUTER JOIN ALUMNOS A ON A.COD_ALU = P.COD_ALU AND A.CARRE = P.CARRE
        LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = P.COD_MAT AND M.CODCARRE = P.CARRE
        WHERE P.CARRE = @Carre AND P.COD_ALU = @CodAlu
        ORDER BY P.MESA, P.COD_MAT
        """;

    private readonly FbConnectionFactory _connectionFactory;

    public CargaFinalCandidatosQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CargaFinalAlumnoDto>> ObtenerAsync(
        int mesa, string codigoCarrera, string tipoExamen, CancellationToken ct)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<CargaFinalAlumnoDto>(new CommandDefinition(
            Sql,
            new { Mesa = mesa, Carre = codigoCarrera, Tipo = tipoExamen },
            cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }

    public async Task<IReadOnlyList<CargaFinalAlumnoDto>> ObtenerPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<CargaFinalAlumnoDto>(new CommandDefinition(
            SqlPorAlumno,
            new { Carre = codigoCarrera, CodAlu = codigoAlumno },
            cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
