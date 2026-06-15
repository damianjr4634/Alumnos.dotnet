using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Persistence.Repositories;

/// <summary>
/// PERMEXA por Dapper. Replica las consultas de PermisoExamen.pas: listado por
/// alumno (join MATERIAS), alta (INDICE/FECH_EMI los pone la base) y baja por la
/// clave de negocio.
/// </summary>
public sealed class PermisosExamenRepository : IPermisosExamenRepository
{
    private readonly FbConnectionFactory _connectionFactory;

    public PermisosExamenRepository(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PermisoExamenDto>> ListarPorAlumnoAsync(
        string codigoCarrera, string codigoAlumno, CancellationToken ct)
    {
        const string sql = """
            SELECT P.PERM_EXA   AS NumeroPermiso,
                   P.MESA       AS Mesa,
                   P.LLAMADO    AS Llamado,
                   P.CUTUCO     AS Cutuco,
                   TRIM(P.COD_MAT) AS CodigoMateria,
                   TRIM(M.SIGLA)   AS SiglaMateria,
                   TRIM(M.DESCRIPCI) AS Materia,
                   P.FECH_EXA   AS FechaExamen,
                   P.FECH_EMI   AS FechaEmision
            FROM PERMEXA P
            LEFT OUTER JOIN MATERIAS M ON P.COD_MAT = M.CODMATERI AND M.CODCARRE = P.CARRE
            WHERE P.COD_ALU = @CodAlu AND P.CARRE = @Carre
            ORDER BY P.MESA, P.COD_MAT
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<PermisoExamenDto>(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }

    public async Task<bool> ExisteAsync(
        string codigoCarrera, string codigoAlumno, int mesa, string codigoMateria, CancellationToken ct)
    {
        const string sql = """
            SELECT COUNT(*) FROM PERMEXA
            WHERE COD_ALU = @CodAlu AND CARRE = @Carre AND MESA = @Mesa AND COD_MAT = @CodMat
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var cantidad = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { CodAlu = codigoAlumno, Carre = codigoCarrera, Mesa = mesa, CodMat = codigoMateria },
            cancellationToken: ct)).ConfigureAwait(false);

        return cantidad > 0;
    }

    public async Task InsertarAsync(CrearPermisoExamenCommand permiso, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO PERMEXA (PERM_EXA, MESA, COD_ALU, APELLIDO, CUTUCO, CARRE, COD_MAT, FECH_EMI, USUARIO)
            VALUES (@PermExa, @Mesa, @CodAlu, @Apellido, @Cutuco, @Carre, @CodMat, CURRENT_DATE, @Usuario)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                PermExa = permiso.NumeroPermiso,
                Mesa = permiso.Mesa,
                CodAlu = permiso.CodigoAlumno,
                Apellido = permiso.Apellido,
                Cutuco = permiso.Cutuco,
                Carre = permiso.CodigoCarrera,
                CodMat = permiso.CodigoMateria,
                Usuario = permiso.CodigoUsuario.ToString(System.Globalization.CultureInfo.InvariantCulture),
            },
            cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task<int> EliminarAsync(string codigoCarrera, string codigoAlumno, string codigoMateria, CancellationToken ct)
    {
        const string sql = "DELETE FROM PERMEXA WHERE COD_ALU = @CodAlu AND CARRE = @Carre AND COD_MAT = @CodMat";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.ExecuteAsync(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera, CodMat = codigoMateria }, cancellationToken: ct))
            .ConfigureAwait(false);
    }
}
