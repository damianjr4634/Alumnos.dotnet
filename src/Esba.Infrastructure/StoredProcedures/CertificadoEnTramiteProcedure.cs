using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT FERRCOD, FERRMSG, FCUATRI FROM XXX_IMPRIME_CTT(@CodAlu, @Carre).
///
/// // TODO-migrar (prioridad media): recorre las MATERIAS de la carrera y verifica
/// // que el alumno tenga cada una aprobada en ANALITIC (contemplando equivalencias
/// // y materias anuales). Si la carrera tiene título intermedio (CUATRIM2) y solo
/// // faltan materias del segundo ciclo, devuelve FERRCOD=1 con el tope FCUATRI.
/// </summary>
public sealed class CertificadoEnTramiteProcedure : ICertificadoEnTramiteProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public CertificadoEnTramiteProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<int>> VerificarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        const string sql = "SELECT FERRCOD, FERRMSG, FCUATRI FROM XXX_IMPRIME_CTT(@CodAlu, @Carre)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QueryFirstOrDefaultAsync<Fila>(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        var ferrcod = fila?.FErrCod ?? 0;
        var ferrmsg = fila?.FErrMsg;
        var fcuatri = fila?.FCuatri ?? 0;

        // No se usa Result.DesdeErrCod porque FCUATRI debe viajar como valor también
        // en el caso NeedsConfirmation (título intermedio), donde DesdeErrCod no lo lleva.
        return ferrcod switch
        {
            2 => Result.Error<int>(ferrmsg ?? "No se puede emitir la constancia."),
            1 => new Result<int>
            {
                Status = OperationStatus.NeedsConfirmation,
                Message = ferrmsg,
                Value = fcuatri,
            },
            _ => Result.Ok(fcuatri),
        };
    }

    private sealed record Fila
    {
        public int FErrCod { get; init; }

        public string? FErrMsg { get; init; }

        public int FCuatri { get; init; }
    }
}
