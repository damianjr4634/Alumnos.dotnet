using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT COLUMNA1, COLUMNA2 FROM XXX_IMPRESION_EQ_BAC(@CodAlu, @Carre).
///
/// El SP escribe el listado en la GTT <c>TMP_EQUI</c> (ON COMMIT DELETE ROWS) y la
/// re-lee en dos columnas. Dapper abre y cierra la conexión por consulta (sin
/// transacción explícita ⇒ autocommit), de modo que la GTT queda vacía para la próxima
/// invocación: no acumula filas entre llamadas.
/// </summary>
public sealed class ImpresionEquivalenciaBachillerProcedure : IEquivalenciaBachillerProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public ImpresionEquivalenciaBachillerProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<LineaEquivalenciaBachillerDto>> ListarLineasAsync(
        string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        const string sql = "SELECT COLUMNA1 AS Columna1, COLUMNA2 AS Columna2 FROM XXX_IMPRESION_EQ_BAC(@CodAlu, @Carre)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<LineaEquivalenciaBachillerDto>(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
