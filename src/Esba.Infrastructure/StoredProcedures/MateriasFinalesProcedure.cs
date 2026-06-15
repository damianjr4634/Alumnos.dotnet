using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT ... FROM XXX_MATERIAS_FINALES(@CodAlu, @Carre).
///
/// // TODO-migrar (prioridad alta): el PSQL resuelve la correlatividad de final
/// // (CORRELATIV/CORREFINAL contra el analítico), condición CTT/CA/DNI y la mesa
/// // vigente. Es lógica de negocio central; portarla requiere el analítico.
/// </summary>
public sealed class MateriasFinalesProcedure : IMateriasFinalesProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public MateriasFinalesProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MateriaFinalDto>> ListarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(CODMAT)  AS CodigoMateria,
                   TRIM(MATERIA) AS Materia,
                   IIF(FERRCOD = 0, TRUE, FALSE) AS PuedeRendir,
                   TRIM(FERRMSG) AS Mensaje,
                   CUTUCO        AS Cutuco,
                   TRIM(CONDICION) AS Condicion,
                   FMESA         AS Mesa
            FROM XXX_MATERIAS_FINALES(@CodAlu, @Carre)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<MateriaFinalDto>(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
