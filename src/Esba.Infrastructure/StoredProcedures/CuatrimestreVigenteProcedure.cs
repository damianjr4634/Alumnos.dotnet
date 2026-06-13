using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT FCUATRIM FROM XXX_NUMCUATANIO(carre) vía Dapper.
///
/// // TODO-migrar (prioridad baja): la lógica PSQL determina el cuatrimestre o
/// // trimestre vigente según la fecha actual contra TBL_CUAT/TBL_TRIM y la
/// // modalidad de la carrera. Portarla a C# es directo (los ciclos lectivos ya
/// // están modelados como CicloCuatrimestral/CicloTrimestral).
/// </summary>
public sealed class CuatrimestreVigenteProcedure : ICuatrimestreVigenteProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public CuatrimestreVigenteProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<string?> ObtenerAsync(string codigoCarrera, CancellationToken ct)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var cuatrimestre = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
            "SELECT FCUATRIM FROM XXX_NUMCUATANIO(@Carre)",
            new { Carre = codigoCarrera },
            cancellationToken: ct)).ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(cuatrimestre) ? null : cuatrimestre.Trim();
    }
}
