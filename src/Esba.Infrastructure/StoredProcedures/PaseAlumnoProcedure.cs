using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT FERRCOD, FERRMSG FROM XXX_IMPRIME_PASE(@CodAlu, @Carre).
///
/// // TODO-migrar (prioridad media): recorre las MATERIAS de la carrera; si el
/// // alumno aprobó TODAS en ANALITIC devuelve FERRCOD=2 (no corresponde pase),
/// // si adeuda alguna devuelve FERRCOD=0 (corresponde).
/// </summary>
public sealed class PaseAlumnoProcedure : IPaseAlumnoProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public PaseAlumnoProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<bool>> VerificarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        const string sql = "SELECT FERRCOD, FERRMSG FROM XXX_IMPRIME_PASE(@CodAlu, @Carre)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QueryFirstOrDefaultAsync<(int FErrCod, string? FErrMsg)>(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return Result.DesdeErrCod(fila.FErrCod, fila.FErrMsg, true);
    }
}
