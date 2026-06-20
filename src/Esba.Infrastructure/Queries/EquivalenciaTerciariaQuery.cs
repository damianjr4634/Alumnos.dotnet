using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Queries de la resolución de equivalencia terciaria (formato nuevo de
/// lst_impresion_equivalencia_terc.pas). El acta interna se formatea en SQL como en el
/// legacy: <c>cast(ACTINT as integer)</c> (quita ceros a la izquierda) y se separan los
/// dos últimos dígitos como año.
/// </summary>
public sealed class EquivalenciaTerciariaQuery : IEquivalenciaTerciariaQuery
{
    // Expresión de formateo del acta interna, compartida por ambas consultas.
    private const string ActaFormateada =
        "SUBSTRING(CAST(A.ACTINT AS INTEGER) FROM 1 FOR CHAR_LENGTH(CAST(A.ACTINT AS INTEGER)) - 2) || '/' || " +
        "SUBSTRING(CAST(A.ACTINT AS INTEGER) FROM CHAR_LENGTH(CAST(A.ACTINT AS INTEGER)) - 1)";

    private readonly FbConnectionFactory _connectionFactory;

    public EquivalenciaTerciariaQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<EncabezadoResolucionTerciariaDto?> ObtenerEncabezadoAsync(
        string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        var sql = $"""
            SELECT EXTRACT(YEAR FROM CURRENT_DATE)                  AS AnioActual,
                   REPLACE(TRIM(L.COD_ALU), 'DNI', 'DNI ')          AS CodigoAlumno,
                   TRIM(L.APELLIDO) || ' ' || TRIM(L.NOM_APE)       AS NombreAlumno,
                   (SELECT LIST(DISTINCT {ActaFormateada})
                      FROM ANALITIC A
                      WHERE A.COD_ALU = L.COD_ALU AND A.CARRE = L.CARRE AND A.CONDICION = 'EQUIVALENCIA') AS ActasInternas
            FROM ALUMNOS L
            WHERE L.COD_ALU = @CodAlu AND L.CARRE = @Carre
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<EncabezadoResolucionTerciariaDto>(
            new CommandDefinition(sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<MateriaEquivalenciaTerciariaDto>> ListarMateriasAsync(
        string codigoAlumno, string codigoCarrera, IReadOnlyCollection<int> cuatrimestres, CancellationToken ct)
    {
        if (cuatrimestres.Count == 0)
        {
            return [];
        }

        // El legacy usa "'2,3' CONTAINING m.cuatrim" (match por substring); acá filtramos
        // con una lista de enteros (IN), que evita el falso positivo de cuatrimestres de
        // más de un dígito.
        var sql = $"""
            SELECT TRIM(M.DESCRIPCI)   AS Descripcion,
                   M.CUATRIM           AS Cuatrimestre,
                   TRIM(D.DOCENTE)     AS Docente,
                   TRIM(A.FEQMATE)     AS MateriaOrigen,
                   TRIM(A.FEQCARRE)    AS CarreraOrigen,
                   TRIM(A.FEQINST)     AS InstitutoOrigen,
                   {ActaFormateada}    AS ActaInterna
            FROM ANALITIC A
            LEFT OUTER JOIN MATERIAS M ON A.COD_MAT = M.CODMATERI AND M.CODCARRE = A.CARRE
            LEFT OUTER JOIN DOCENTES D ON D.CODPROFES = A.FEQDOCE
            WHERE A.COD_ALU = @CodAlu AND A.CARRE = @Carre AND A.CONDICION = 'EQUIVALENCIA'
              AND M.CUATRIM IN @Cuatrimestres
            ORDER BY M.CUATRIM, M.ORDEN
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<MateriaEquivalenciaTerciariaDto>(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera, Cuatrimestres = cuatrimestres }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
