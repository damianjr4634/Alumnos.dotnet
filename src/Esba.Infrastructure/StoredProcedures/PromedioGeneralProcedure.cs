using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT PROMGRAL FROM XXX_PROMEDIO_GRAL(@CodAlu, @Carre) vía Dapper.
///
/// // TODO-migrar (prioridad baja): el PSQL es un AVG(NOTA_MAT) sobre ANALITIC
/// // filtrando COD_ALU+CARRE y descartando notas en 0, con COALESCE a 0. Portarlo
/// // a C# es directo una vez modelado el analítico (hito 14).
/// </summary>
public sealed class PromedioGeneralProcedure : IPromedioGeneralProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public PromedioGeneralProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<decimal> ObtenerAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var promedio = await connection.ExecuteScalarAsync<decimal?>(new CommandDefinition(
            "SELECT PROMGRAL FROM XXX_PROMEDIO_GRAL(@CodAlu, @Carre)",
            new { CodAlu = codigoAlumno, Carre = codigoCarrera },
            cancellationToken: ct)).ConfigureAwait(false);

        return promedio ?? 0m;
    }
}
