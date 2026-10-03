using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.AreaDocente;
using Esba.Domain.Enums;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Pantalla de precarga de una mesa (hito 19). La mesa con su cabecera de carga (MESAS +
/// CARRERA + MATERIAS + MESA_TIPO + DOCENTES + DOC_CARGA_MESA) y los alumnos con permiso
/// de examen (PERMEXA de la mesa, no dados de baja — mismo universo que la cantidad del
/// inicio docente) con su condición actual en CURSADA y la fila de DOC_CARGA_MESA_DET si
/// existe. Secretaría, al efectivizar, usa su propio candidato (XXX_MESAS_ALUMNOS, hito 14).
/// </summary>
public sealed class CargaMesaDocenteQuery : ICargaMesaDocenteQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public CargaMesaDocenteQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CargaMesaDocenteDto?> ObtenerAsync(ClaveMesa mesa, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(mesa);

        const string sqlCabecera = """
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
                   TRIM(M.TITULAR)   AS CodigoDocenteTitular,
                   TRIM(D.DOCENTE)   AS NombreDocenteTitular,
                   TRIM(M.VOCAL1)    AS Vocal1,
                   TRIM(M.VOCAL2)    AS Vocal2,
                   DM.ID             AS CargaId,
                   TRIM(DM.ESTADO)   AS EstadoCodigo,
                   DM.OBSERV         AS ObservacionesCarga,
                   DM.FEC_MODIF      AS FechaModificacion,
                   DM.FEC_FINAL      AS FechaFinalizacion,
                   DM.FEC_EFECTIVO   AS FechaEfectivizacion
            FROM MESAS M
            LEFT OUTER JOIN CARRERA K ON K.CARRE = M.CARRE
            LEFT OUTER JOIN MATERIAS A ON A.CODMATERI = M.COD_MAT AND A.CODCARRE = M.CARRE
            LEFT OUTER JOIN MESA_TIPO T ON T.CODIGO = M.TIPMES
            LEFT OUTER JOIN DOCENTES D ON D.CODPROFES = M.TITULAR
            LEFT OUTER JOIN DOC_CARGA_MESA DM ON DM.CARRE = M.CARRE AND DM.MESA = M.MESA
            WHERE M.CARRE = @Carre AND M.MESA = @Mesa
            """;

        const string sqlAlumnos = """
            SELECT TRIM(P.COD_ALU)   AS CodigoAlumno,
                   TRIM(AL.APELLIDO) AS Apellido,
                   TRIM(AL.NOM_APE)  AS Nombre,
                   TRIM(P.COD_MAT)   AS CodigoMateria,
                   TRIM(C.CONDICION) AS Condicion,
                   P.INDICE          AS PermisoIndice,
                   P.LLAMADO         AS Llamado,
                   DET.ID            AS DetalleId,
                   DET.NOTA          AS Nota,
                   IIF(DET.AUSENTE = 'S', TRUE, FALSE) AS Ausente,
                   DET.OBSERV        AS Observaciones
            FROM PERMEXA P
            LEFT OUTER JOIN ALUMNOS AL ON AL.COD_ALU = P.COD_ALU AND AL.CARRE = P.CARRE
            LEFT OUTER JOIN CURSADA C ON C.COD_ALU = P.COD_ALU AND C.COD_MAT = P.COD_MAT AND C.CARRE = P.CARRE
            LEFT OUTER JOIN DOC_CARGA_MESA DM ON DM.CARRE = P.CARRE AND DM.MESA = P.MESA
            LEFT OUTER JOIN DOC_CARGA_MESA_DET DET
                   ON DET.CARGA_ID = DM.ID AND DET.COD_ALU = P.COD_ALU AND DET.COD_MAT = P.COD_MAT
            WHERE P.CARRE = @Carre AND P.MESA = @Mesa AND AL.BAJA = 'N'
            ORDER BY AL.APELLIDO, AL.NOM_APE, P.COD_ALU, P.COD_MAT
            """;

        var parametros = new { Carre = mesa.CodigoCarrera.Trim(), Mesa = mesa.NumeroMesa };

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        var cabecera = await connection.QuerySingleOrDefaultAsync<CabeceraRow>(
            new CommandDefinition(sqlCabecera, parametros, cancellationToken: ct)).ConfigureAwait(false);
        if (cabecera is null)
        {
            return null;
        }

        var alumnos = await connection.QueryAsync<AlumnoCargaMesaDto>(
            new CommandDefinition(sqlAlumnos, parametros, cancellationToken: ct)).ConfigureAwait(false);

        return new CargaMesaDocenteDto
        {
            CodigoCarrera = cabecera.CodigoCarrera,
            NombreCarrera = cabecera.NombreCarrera,
            NumeroMesa = cabecera.NumeroMesa,
            CodigoMateria = cabecera.CodigoMateria,
            SiglaMateria = cabecera.SiglaMateria,
            NombreMateria = cabecera.NombreMateria,
            Llamado = cabecera.Llamado,
            FechaExamen = cabecera.FechaExamen,
            Hora = cabecera.Hora,
            Aula = cabecera.Aula,
            DescripcionTipo = cabecera.DescripcionTipo,
            CodigoDocenteTitular = cabecera.CodigoDocenteTitular,
            NombreDocenteTitular = cabecera.NombreDocenteTitular,
            Vocal1 = cabecera.Vocal1,
            Vocal2 = cabecera.Vocal2,
            CargaId = cabecera.CargaId,
            Estado = EstadoCargaDocenteCodigo.DesdeCodigoOpcional(cabecera.EstadoCodigo),
            ObservacionesCarga = cabecera.ObservacionesCarga,
            FechaModificacion = cabecera.FechaModificacion,
            FechaFinalizacion = cabecera.FechaFinalizacion,
            FechaEfectivizacion = cabecera.FechaEfectivizacion,
            Alumnos = alumnos.AsList(),
        };
    }

    // Fila cruda de la cabecera. Clase con propiedades (no record posicional): Dapper
    // materializa por propiedades con conversión de tipos (NUMERIC → int?, DATE → DateOnly?
    // vía el type handler), mientras que un constructor exige tipos idénticos.
    private sealed class CabeceraRow
    {
        public required string CodigoCarrera { get; init; }
        public string? NombreCarrera { get; init; }
        public int NumeroMesa { get; init; }
        public string? CodigoMateria { get; init; }
        public string? SiglaMateria { get; init; }
        public string? NombreMateria { get; init; }
        public int? Llamado { get; init; }
        public DateOnly? FechaExamen { get; init; }
        public int? Hora { get; init; }
        public int? Aula { get; init; }
        public string? DescripcionTipo { get; init; }
        public string? CodigoDocenteTitular { get; init; }
        public string? NombreDocenteTitular { get; init; }
        public string? Vocal1 { get; init; }
        public string? Vocal2 { get; init; }
        public int? CargaId { get; init; }
        public string? EstadoCodigo { get; init; }
        public string? ObservacionesCarga { get; init; }
        public DateTime? FechaModificacion { get; init; }
        public DateTime? FechaFinalizacion { get; init; }
        public DateTime? FechaEfectivizacion { get; init; }
    }
}
