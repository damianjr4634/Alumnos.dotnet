using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Docente;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Lecturas del área docente (hito 19). Sin equivalente legacy (el escritorio no tenía
/// vista por docente): reutiliza los joins de ComisionesQuery/MesasQuery y agrega el
/// LEFT JOIN a las tablas de precarga DOC_CARGA_* (migración 2026-10-03). El orden
/// cronológico de CUA_ANIO ("cuatrimestre + año") se resuelve por año y luego
/// cuatrimestre; los códigos fuera del patrón quedan al final.
/// </summary>
public sealed class AreaDocenteQuery : IAreaDocenteQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public AreaDocenteQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ComisionDocenteDto>> ListarComisionesAsync(string codigoDocente, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoDocente);

        const string sql = """
            SELECT TRIM(C.CARRE)     AS CodigoCarrera,
                   TRIM(K.DESCORT)   AS NombreCarrera,
                   C.CUTUCO          AS Cutuco,
                   TRIM(C.COD_MAT)   AS CodigoMateria,
                   TRIM(M.SIGLA)     AS SiglaMateria,
                   TRIM(M.DESCRIPCI) AS NombreMateria,
                   TRIM(C.CUA_ANIO)  AS CuatrimestreAnio,
                   TRIM(C.DIA1)      AS Dia1,
                   TRIM(C.BLOQUE1)   AS Bloque1,
                   TRIM(C.DIA2)      AS Dia2,
                   TRIM(C.BLOQUE2)   AS Bloque2,
                   TRIM(C.DIA3)      AS Dia3,
                   TRIM(C.BLOQUE3)   AS Bloque3,
                   (SELECT COUNT(*)
                      FROM CURSADA U
                     WHERE U.CARRE = C.CARRE AND U.CUTUCO = C.CUTUCO
                       AND U.COD_MAT = C.COD_MAT AND U.CUA_ANIO = C.CUA_ANIO
                       AND TRIM(U.CONDICION) IN ('CURSANDO', 'RECURSANDO')) AS CantidadAlumnos,
                   TRIM(DC.ESTADO)   AS EstadoCarga
            FROM COMARM C
            LEFT OUTER JOIN CARRERA K ON K.CARRE = C.CARRE
            LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = C.COD_MAT AND M.CODCARRE = C.CARRE
            LEFT OUTER JOIN DOC_CARGA_COMISION DC
                   ON DC.CARRE = C.CARRE AND DC.CUTUCO = C.CUTUCO
                  AND DC.COD_MAT = C.COD_MAT AND DC.CUA_ANIO = C.CUA_ANIO
            WHERE C.CODPROFES = @CodProfes
            ORDER BY SUBSTRING(C.CUA_ANIO FROM 2 FOR 2) DESC,
                     SUBSTRING(C.CUA_ANIO FROM 1 FOR 1) DESC,
                     K.DESCORT, M.SIGLA, C.CUTUCO
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<ComisionDocenteDto>(new CommandDefinition(
            sql, new { CodProfes = codigoDocente.Trim() }, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }

    public async Task<IReadOnlyList<MesaDocenteDto>> ListarMesasAsync(string codigoDocente, DateOnly? desde, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoDocente);

        // El recorte por fecha se arma condicionalmente: Firebird no puede tipar un
        // parámetro usado solo en "@p IS NULL".
        var filtroFecha = desde is null ? string.Empty : "AND M.FECH_EXA >= @Desde";
        var sql = $"""
            SELECT TRIM(M.CARRE)     AS CodigoCarrera,
                   TRIM(K.DESCORT)   AS NombreCarrera,
                   M.MESA            AS NumeroMesa,
                   TRIM(M.COD_MAT)   AS CodigoMateria,
                   TRIM(A.SIGLA)     AS SiglaMateria,
                   TRIM(A.DESCRIPCI) AS NombreMateria,
                   M.LLAMADO         AS Llamado,
                   M.FECH_EXA        AS FechaExamen,
                   M.HORA            AS Hora,
                   M.AULA            AS Aula,
                   TRIM(T.DESCRI)    AS DescripcionTipo,
                   (SELECT COUNT(*) FROM PERMEXA P
                     WHERE P.CARRE = M.CARRE AND P.MESA = M.MESA) AS CantidadInscriptos,
                   TRIM(DM.ESTADO)   AS EstadoCarga
            FROM MESAS M
            LEFT OUTER JOIN CARRERA K ON K.CARRE = M.CARRE
            LEFT OUTER JOIN MATERIAS A ON A.CODMATERI = M.COD_MAT AND A.CODCARRE = M.CARRE
            LEFT OUTER JOIN MESA_TIPO T ON T.CODIGO = M.TIPMES
            LEFT OUTER JOIN DOC_CARGA_MESA DM ON DM.CARRE = M.CARRE AND DM.MESA = M.MESA
            WHERE M.TITULAR = @CodProfes
              {filtroFecha}
            ORDER BY M.FECH_EXA DESC, M.HORA, K.DESCORT, A.SIGLA
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<MesaDocenteDto>(new CommandDefinition(
            sql, new { CodProfes = codigoDocente.Trim(), Desde = desde }, cancellationToken: ct)).ConfigureAwait(false);
        return filas.AsList();
    }
}
