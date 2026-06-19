using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT FERRCOD, FERRMSG FROM XXX_INSC_VALMAT(@CodAlu, @Carre, @CodMat, @Tipo).
///
/// // TODO-migrar (prioridad media): valida que la materia no esté ya en CURSADA ni
/// // ANALITIC (FERRCOD=2) y, solo para TIPO='I', que estén aprobadas las
/// // correlatividades (MATERIAS.CORRELATIV). Lógica de negocio del alta de cursada
/// // y de equivalencias.
/// </summary>
public sealed class ValidacionMateriaProcedure : IValidacionMateriaProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public ValidacionMateriaProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<bool>> ValidarAsync(
        string codigoAlumno, string codigoCarrera, string codigoMateria, char tipo, CancellationToken ct)
    {
        const string sql = "SELECT FERRCOD, FERRMSG FROM XXX_INSC_VALMAT(@CodAlu, @Carre, @CodMat, @Tipo)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QueryFirstOrDefaultAsync<(int FErrCod, string? FErrMsg)>(new CommandDefinition(
            sql,
            new
            {
                CodAlu = codigoAlumno,
                Carre = codigoCarrera,
                CodMat = codigoMateria,
                Tipo = tipo.ToString(),
            },
            cancellationToken: ct)).ConfigureAwait(false);

        return Result.DesdeErrCod(fila.FErrCod, fila.FErrMsg, true);
    }
}
