using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Cursada de un alumno (reescritura parametrizada del SELECT de
/// DataModule.IBInscMaterias: CURSADA + JOIN MATERIAS).
/// </summary>
public sealed class CursadaQuery : ICursadaQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public CursadaQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CursadaListItemDto>> ListarPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(C.CARRE)     AS CodigoCarrera,
                   TRIM(C.COD_ALU)   AS CodigoAlumno,
                   TRIM(C.COD_MAT)   AS CodigoMateria,
                   TRIM(COALESCE(M.SIGLA, M.DESCRIPCI)) AS SiglaMateria,
                   C.CUTUCO          AS Cutuco,
                   TRIM(C.CUA_ANIO)  AS CuatrimestreAnio,
                   TRIM(C.CONDICION) AS Condicion,
                   C.TP_EVA          AS Evaluacion1,
                   C.TP_EVA2         AS Evaluacion2,
                   C.REGULAR         AS NotaRegular,
                   C.FINAL1          AS NotaFinal1,
                   C.FECHA1          AS FechaFinal1,
                   C.INASIST         AS Inasistencias
            FROM CURSADA C
            JOIN MATERIAS M ON M.CODMATERI = C.COD_MAT AND M.CODCARRE = C.CARRE
            WHERE C.CARRE = @Carre AND C.COD_ALU = @CodAlu
            ORDER BY C.CUA_ANIO DESC, M.CUATRIM, C.COD_MAT
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<CursadaListItemDto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera, CodAlu = codigoAlumno }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
