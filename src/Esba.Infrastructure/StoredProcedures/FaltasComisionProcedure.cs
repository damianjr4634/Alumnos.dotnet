using System.Globalization;
using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT ... FROM XXX_FALTAS_COMISION(@Carre,@Cutuco,@Fecha,@CuaAnio,@CodMat).
///
/// // TODO-migrar (prioridad media): el PSQL arma el padrón de la comisión y suma
/// // las faltas acumuladas de cada alumno; portarlo requiere antes el modelo de
/// // FALTAS y la lógica de acumulación por tipo (TBL_FALTAS).
/// </summary>
public sealed class FaltasComisionProcedure : IFaltasComisionProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public FaltasComisionProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AlumnoComisionFaltasDto>> ListarAsync(
        string codigoCarrera, short cutuco, string cuatrimestreAnio, string? codigoMateria, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(CODALU) AS CodigoAlumno,
                   TRIM(NOMBRE) AS Nombre,
                   CANANT       AS CantidadAnterior,
                   TRIM(CODFAL) AS CodigoFalta,
                   CANTID       AS Cantidad
            FROM XXX_FALTAS_COMISION(@Carre, @Cutuco, NULL, @CuaAnio, @CodMat)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<AlumnoComisionFaltasDto>(new CommandDefinition(
            sql,
            new
            {
                Carre = codigoCarrera,
                Cutuco = cutuco.ToString(CultureInfo.InvariantCulture),
                CuaAnio = cuatrimestreAnio,
                CodMat = codigoMateria,
            },
            cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
