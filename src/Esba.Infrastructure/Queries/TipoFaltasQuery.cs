using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

/// <summary>
/// Catálogo de tipos de inasistencia. Reescritura parametrizada del lookup de
/// CargaInasistenciasComisionNuevo.MemoClick (TBL_FALTAS filtrado por carrera).
/// </summary>
public sealed class TipoFaltasQuery : ITipoFaltasQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public TipoFaltasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<TipoFaltaDto>> ListarPorCarreraAsync(string codigoCarrera, CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(FCODIGO) AS Codigo,
                   TRIM(FDESCRI) AS Descripcion,
                   FCANTID       AS Cantidad,
                   IIF(FJUSTIF = 'S', TRUE, FALSE) AS Justifica
            FROM TBL_FALTAS
            WHERE CARRE IS NULL OR CARRE CONTAINING @Carre
            ORDER BY FCODIGO
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<TipoFaltaDto>(new CommandDefinition(
            sql, new { Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
