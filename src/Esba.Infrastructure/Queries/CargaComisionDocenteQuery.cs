using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Enums;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Pantalla de precarga de una comisión (hito 19). Dos lecturas en la misma conexión:
/// la comisión con su cabecera de carga (COMARM + CARRERA + MATERIAS + DOCENTES +
/// DOC_CARGA_COMISION) y los alumnos cursando/recursando no dados de baja (mismo
/// universo que la cantidad del inicio docente) con los valores actuales de CURSADA
/// y la fila de DOC_CARGA_COMISION_DET si existe.
/// </summary>
public sealed class CargaComisionDocenteQuery : ICargaComisionDocenteQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public CargaComisionDocenteQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CargaComisionDocenteDto?> ObtenerAsync(ClaveComision comision, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comision);

        const string sqlCabecera = """
            SELECT TRIM(C.CARRE)     AS CodigoCarrera,
                   TRIM(K.DESCORT)   AS NombreCarrera,
                   TRIM(K.TIPO)      AS TipoCarrera,
                   C.CUTUCO          AS Cutuco,
                   TRIM(C.COD_MAT)   AS CodigoMateria,
                   TRIM(M.SIGLA)     AS SiglaMateria,
                   TRIM(M.DESCRIPCI) AS NombreMateria,
                   TRIM(C.CUA_ANIO)  AS CuatrimestreAnio,
                   TRIM(C.CODPROFES) AS CodigoDocenteTitular,
                   TRIM(D.DOCENTE)   AS NombreDocenteTitular,
                   DC.ID             AS CargaId,
                   TRIM(DC.ESTADO)   AS EstadoCodigo,
                   DC.OBSERV         AS ObservacionesCarga,
                   DC.FEC_MODIF      AS FechaModificacion,
                   DC.FEC_FINAL      AS FechaFinalizacion,
                   DC.FEC_EFECTIVO   AS FechaEfectivizacion
            FROM COMARM C
            LEFT OUTER JOIN CARRERA K ON K.CARRE = C.CARRE
            LEFT OUTER JOIN MATERIAS M ON M.CODMATERI = C.COD_MAT AND M.CODCARRE = C.CARRE
            LEFT OUTER JOIN DOCENTES D ON D.CODPROFES = C.CODPROFES
            LEFT OUTER JOIN DOC_CARGA_COMISION DC
                   ON DC.CARRE = C.CARRE AND DC.CUTUCO = C.CUTUCO
                  AND DC.COD_MAT = C.COD_MAT AND DC.CUA_ANIO = C.CUA_ANIO
            WHERE C.CARRE = @Carre AND C.CUTUCO = @Cutuco AND C.COD_MAT = @CodMat AND C.CUA_ANIO = @CuaAnio
            """;

        const string sqlAlumnos = """
            SELECT TRIM(U.COD_ALU)   AS CodigoAlumno,
                   TRIM(A.APELLIDO)  AS Apellido,
                   TRIM(A.NOM_APE)   AS Nombre,
                   TRIM(U.CONDICION) AS Condicion,
                   U.INDICE          AS CursadaIndice,
                   U.TP_EVA          AS CursadaEvaluacion1,
                   U.RECUP           AS CursadaRecuperatorio,
                   U.TP_EVA2         AS CursadaEvaluacion2,
                   U.TP_EVA3         AS CursadaEvaluacion3,
                   U.REGULAR         AS CursadaNotaRegular,
                   U.FINAL1          AS CursadaNotaFinal,
                   U.TOT_HORAS       AS CursadaTotalHoras,
                   U.INASIST         AS CursadaInasistencias,
                   U.JUSTIF          AS CursadaJustificadas,
                   DET.ID            AS DetalleId,
                   DET.TP_EVA        AS Evaluacion1,
                   DET.RECUP         AS Recuperatorio,
                   DET.TP_EVA2       AS Evaluacion2,
                   DET.TP_EVA3       AS Evaluacion3,
                   DET.REGULAR       AS NotaRegular,
                   DET.FINAL1        AS NotaFinal,
                   DET.TOT_HORAS     AS TotalHoras,
                   DET.INASIST       AS Inasistencias,
                   DET.JUSTIF        AS Justificadas,
                   DET.OBSERV        AS Observaciones
            FROM CURSADA U
            LEFT OUTER JOIN ALUMNOS A ON A.COD_ALU = U.COD_ALU AND A.CARRE = U.CARRE
            LEFT OUTER JOIN DOC_CARGA_COMISION DC
                   ON DC.CARRE = U.CARRE AND DC.CUTUCO = U.CUTUCO
                  AND DC.COD_MAT = U.COD_MAT AND DC.CUA_ANIO = U.CUA_ANIO
            LEFT OUTER JOIN DOC_CARGA_COMISION_DET DET ON DET.CARGA_ID = DC.ID AND DET.COD_ALU = U.COD_ALU
            WHERE U.CARRE = @Carre AND U.CUTUCO = @Cutuco AND U.COD_MAT = @CodMat AND U.CUA_ANIO = @CuaAnio
              AND TRIM(U.CONDICION) IN ('CURSANDO', 'RECURSANDO') AND A.BAJA = 'N'
            ORDER BY A.APELLIDO, A.NOM_APE, U.COD_ALU
            """;

        var parametros = new
        {
            Carre = comision.CodigoCarrera.Trim(),
            comision.Cutuco,
            CodMat = comision.CodigoMateria.Trim(),
            CuaAnio = comision.CuatrimestreAnio.Trim(),
        };

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var cabecera = await connection.QuerySingleOrDefaultAsync<CabeceraRow>(
            new CommandDefinition(sqlCabecera, parametros, cancellationToken: ct)).ConfigureAwait(false);
        if (cabecera is null)
        {
            return null;
        }

        var alumnos = await connection.QueryAsync<AlumnoCargaComisionDto>(
            new CommandDefinition(sqlAlumnos, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new CargaComisionDocenteDto
        {
            CodigoCarrera = cabecera.CodigoCarrera,
            NombreCarrera = cabecera.NombreCarrera,
            TipoCarrera = cabecera.TipoCarrera,
            Cutuco = cabecera.Cutuco,
            CodigoMateria = cabecera.CodigoMateria,
            SiglaMateria = cabecera.SiglaMateria,
            NombreMateria = cabecera.NombreMateria,
            CuatrimestreAnio = cabecera.CuatrimestreAnio,
            CodigoDocenteTitular = cabecera.CodigoDocenteTitular,
            NombreDocenteTitular = cabecera.NombreDocenteTitular,
            CargaId = cabecera.CargaId,
            Estado = EstadoCargaDocenteCodigo.DesdeCodigoOpcional(cabecera.EstadoCodigo),
            ObservacionesCarga = cabecera.ObservacionesCarga,
            FechaModificacion = cabecera.FechaModificacion,
            FechaFinalizacion = cabecera.FechaFinalizacion,
            FechaEfectivizacion = cabecera.FechaEfectivizacion,
            Alumnos = alumnos.AsList(),
        };
    }

    // Fila cruda de la cabecera: el estado llega como código y se traduce al enum acá.
    private sealed record CabeceraRow(
        string CodigoCarrera,
        string? NombreCarrera,
        string? TipoCarrera,
        short Cutuco,
        string CodigoMateria,
        string? SiglaMateria,
        string? NombreMateria,
        string CuatrimestreAnio,
        string? CodigoDocenteTitular,
        string? NombreDocenteTitular,
        int? CargaId,
        string? EstadoCodigo,
        string? ObservacionesCarga,
        DateTime? FechaModificacion,
        DateTime? FechaFinalizacion,
        DateTime? FechaEfectivizacion);
}
