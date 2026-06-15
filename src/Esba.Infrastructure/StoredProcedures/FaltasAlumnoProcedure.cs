using System.Globalization;
using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT ... FROM XXX_FALTAS_FALTAS(@Carre,@Cutuco,@CuaAnio,@CodAlu,@CodMat).
///
/// // TODO-migrar (prioridad media): el PSQL devuelve las faltas cargadas del
/// // alumno con la descripción del tipo; es un SELECT sobre FALTAS + TBL_FALTAS.
/// </summary>
public sealed class FaltasAlumnoProcedure : IFaltasAlumnoProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public FaltasAlumnoProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<FaltaAlumnoDto>> ListarAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string codigoAlumno, string? codigoMateria, CancellationToken ct)
    {
        const string sql = """
            SELECT FECHA        AS Fecha,
                   TRIM(CODFAL) AS CodigoFalta,
                   CANTID       AS Cantidad,
                   TRIM(DESCRI) AS Descripcion
            FROM XXX_FALTAS_FALTAS(@Carre, @Cutuco, @CuaAnio, @CodAlu, @CodMat)
            ORDER BY FECHA
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<FaltaAlumnoDto>(new CommandDefinition(
            sql,
            new
            {
                Carre = codigoCarrera,
                Cutuco = (int)cutuco,
                CuaAnio = int.Parse(cuatrimestreAnio, CultureInfo.InvariantCulture),
                CodAlu = codigoAlumno,
                CodMat = codigoMateria,
            },
            cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
