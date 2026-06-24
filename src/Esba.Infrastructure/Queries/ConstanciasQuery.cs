using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

public sealed class ConstanciasQuery : IConstanciasQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public ConstanciasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CarreraConstanciaDto?> ObtenerDatosCarreraAsync(string codigoCarrera, CancellationToken ct)
    {
        // STRCuatAnio del legacy: ' AÑO; ' para 333/650, ' CUAT.; ' para el resto.
        const string sql = """
            SELECT TRIM(DESCARRE)  AS Nombre,
                   TRIM(DESCARRE2) AS NombreAlternativo,
                   TRIM(DURACION)  AS Duracion,
                   TRIM(RESOLUCION) AS Resolucion,
                   TRIM(RECTOR)    AS Rector,
                   TRIM(SECRETARIA) AS Secretaria,
                   TRIM(IDIOMA)    AS Idioma,
                   TRIM(INSTITUT)  AS Instituto,
                   TRIM(CARACT)    AS Caracteristica,
                   TRIM(TIPO)      AS Tipo,
                   IIF(TRIM(CARRE) IN ('333', '650'), TRUE, FALSE) AS EsCarreraPorAnio
            FROM CARRERA
            WHERE TRIM(CARRE) = @Carre
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<CarreraConstanciaDto>(
            new CommandDefinition(sql, new { Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<EncabezadoEquivalenciaBachillerDto?> ObtenerEncabezadoEquivalenciaBachillerAsync(
        string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        // SELECT FIRST 1 de lst_impresion_equivalencia_bac.pas, ampliado con CARRERA
        // (nombre largo y TIPO) para que el handler reformatee la resolución y revalide
        // que la carrera sea de bachillerato sin una segunda consulta.
        const string sql = """
            SELECT FIRST 1
                   TRIM(L.APELLIDO) || ' ' || TRIM(L.NOM_APE)   AS Alumno,
                   TRIM(A.ACTINT)                               AS ActividadInterna,
                   TRIM(A.A_C)                                  AS DocumentoAC,
                   TRIM(A.INSTITUT)                             AS Instituto,
                   TRIM(A.COLEGIO)                              AS Colegio,
                   TRIM(COALESCE(T.FDESCRI, A."PLAN"))          AS PlanDescripcion,
                   TRIM(C.DESCARRE)                             AS NombreCarrera,
                   TRIM(C.TIPO)                                 AS TipoCarrera,
                   TRIM(C.INSTITUT)                             AS InstitutoEmisor,
                   TRIM(C.CARACT)                               AS CaracteristicaEmisor
            FROM ANALITIC A
            LEFT OUTER JOIN ALUMNOS L ON L.CARRE = A.CARRE AND L.COD_ALU = A.COD_ALU
            LEFT OUTER JOIN TBLPLANES T ON T.FCODIGO = A."PLAN"
            LEFT OUTER JOIN CARRERA C ON C.CARRE = A.CARRE
            WHERE A.COD_ALU = @CodAlu AND A.CARRE = @Carre AND A.CONDICION = 'EQUIVALENCIA'
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<EncabezadoEquivalenciaBachillerDto>(
            new CommandDefinition(sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<AlumnoRegularDto?> ObtenerAlumnoRegularAsync(
        string codigoAlumno, string codigoCarrera, string cuatrimestreVigente, CancellationToken ct)
    {
        // SELECT de FormShow de constanciaalumnoregular.pas (CURSADA + CARRERA), ampliado
        // con ALUMNOS para el nombre y el mail. Toma la primera fila del orden legacy.
        const string sql = """
            SELECT FIRST 1
                   TRIM(AL.APELLIDO) || ', ' || TRIM(AL.NOM_APE)   AS NombreCompleto,
                   C.CUTUCO                                        AS Cutuco,
                   IIF(TRIM(CA.DISTANCIA) = 'S', TRUE, FALSE)      AS EsADistancia,
                   TRIM(CA.DICTAMEN)                               AS Dictamen,
                   TRIM(AL.MAIL)                                   AS Mail
            FROM CURSADA C
            LEFT OUTER JOIN CARRERA CA ON CA.CARRE = C.CARRE
            LEFT OUTER JOIN ALUMNOS AL ON AL.CARRE = C.CARRE AND AL.COD_ALU = C.COD_ALU
            WHERE C.CARRE = @Carre AND C.COD_ALU = @CodAlu
              AND TRIM(C.CONDICION) IN ('CURSANDO', 'RECURSANDO')
              AND C.CUA_ANIO = @CuaAnio
            ORDER BY C.CUTUCO DESC, C.CONDICION
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<AlumnoRegularDto>(
            new CommandDefinition(
                sql,
                new { CodAlu = codigoAlumno, Carre = codigoCarrera, CuaAnio = cuatrimestreVigente },
                cancellationToken: ct)).ConfigureAwait(false);
    }
}
