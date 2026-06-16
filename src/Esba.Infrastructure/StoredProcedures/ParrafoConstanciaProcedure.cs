using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT FPARRAFO FROM XXX_PARRAFO_CONSTANCIA(@CodAlu, @Carre, @Tipo).
///
/// // TODO-migrar (prioridad media): compone el párrafo legal según el tipo (CTT,
/// // PASE, ANALITICO, CE-xx) a partir de ALUMNOS/CARRERA/ANALITIC y de
/// // YYY_PASA_MAYUS. Para CTT consulta a su vez XXX_IMPRIME_CTT. Es texto, no
/// // afecta datos; portarlo implica replicar el formato exacto de cada variante.
/// </summary>
public sealed class ParrafoConstanciaProcedure : IParrafoConstanciaProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public ParrafoConstanciaProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<string> ObtenerAsync(string codigoAlumno, string codigoCarrera, string tipo, CancellationToken ct)
    {
        const string sql = "SELECT FPARRAFO FROM XXX_PARRAFO_CONSTANCIA(@CodAlu, @Carre, @Tipo)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var parrafo = await connection.QueryFirstOrDefaultAsync<string?>(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera, Tipo = tipo }, cancellationToken: ct)).ConfigureAwait(false);

        return parrafo ?? string.Empty;
    }
}
