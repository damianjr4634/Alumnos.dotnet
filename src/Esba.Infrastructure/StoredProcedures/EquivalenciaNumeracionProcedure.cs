using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// Wrappers de la numeración de equivalencias:
/// SELECT NUM_FORMA, NUM_ENTERO, FERRMSG, FNUMNUE FROM XXX_NUMERO_EQUIVALENCIA(@CodAlu,@Carre)
/// y EXECUTE PROCEDURE XXX_GRABA_NUMEQUI(@Numero,@Carre).
///
/// // TODO-migrar (prioridad baja): la numeración se apoya en TBLEQUIVA (último número
/// // por carrera/año, TER compartido) y en el máximo ACTINT del alumno. Lógica de
/// // secuencia que conviene portar junto con TBLEQUIVA.
/// </summary>
public sealed class EquivalenciaNumeracionProcedure : IEquivalenciaNumeracionProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public EquivalenciaNumeracionProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<NumeroEquivalenciaDto> ObtenerProximoNumeroAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        const string sql = "SELECT NUM_FORMA, NUM_ENTERO, FERRMSG, FNUMNUE FROM XXX_NUMERO_EQUIVALENCIA(@CodAlu, @Carre)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var fila = await connection.QueryFirstOrDefaultAsync<(string? NumForma, string? NumEntero, string? FErrMsg, string? FNumNue)>(
            new CommandDefinition(sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return new NumeroEquivalenciaDto
        {
            NumeroFormateado = fila.NumForma?.Trim() ?? string.Empty,
            NumeroEntero = fila.NumEntero?.Trim() ?? string.Empty,
            Mensaje = string.IsNullOrWhiteSpace(fila.FErrMsg) ? null : fila.FErrMsg.Trim(),
            EsNuevo = string.Equals(fila.FNumNue?.Trim(), "S", StringComparison.OrdinalIgnoreCase),
        };
    }

    public async Task GrabarNumeroAsync(int numero, string codigoCarrera, CancellationToken ct)
    {
        const string sql = "EXECUTE PROCEDURE XXX_GRABA_NUMEQUI(@Numero, @Carre)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            sql, new { Numero = numero, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);
    }
}
