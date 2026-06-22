using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT FERRCOD, FERRMSG FROM YYY_SEGU_GRABA(@CodUsu, @Opciones). Reemplaza los
/// permisos del usuario: el SP borra todo BARRA_SEGU del usuario y reinserta los
/// códigos recibidos. El SP separa los ítems por '&' y toma el código anterior al
/// primer '-', así que el wrapper arma "codigo-" por cada uno (el guión es el
/// delimitador que el SP exige). Una lista vacía deja al usuario sin permisos.
///
/// // TODO-migrar (prioridad baja): el PSQL es un DELETE + un FOR que inserta;
/// // portarlo a EF Core es directo y elimina la dependencia de YYY_SEGU_EXTRAEOPC.
/// // FERRCOD siempre vuelve 0 (el SP no valida), de ahí que el mapeo sea siempre Ok.
/// </summary>
public sealed class SeguGrabaProcedure : ISeguGrabaProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public SeguGrabaProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<int>> GrabarAsync(int codigoUsuario, IReadOnlyList<string> codigosOpcion, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(codigosOpcion);

        // El SP extrae el código con SUBSTRING ... FOR POSITION('-')-1: cada ítem
        // debe contener el '-'. Se une por '&' como en PermisosPorUsuario.pas.
        var opciones = string.Join("&", codigosOpcion.Select(c => c.Trim() + "-"));

        const string sql = """
            SELECT FERRCOD AS FerrCod, FERRMSG AS FerrMsg
            FROM YYY_SEGU_GRABA(@CodUsu, @Opciones)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var transaccion = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        var fila = await connection.QueryFirstOrDefaultAsync<ResultadoRow>(new CommandDefinition(
            sql,
            new { CodUsu = codigoUsuario, Opciones = opciones },
            transaction: transaccion,
            cancellationToken: ct)).ConfigureAwait(false);

        var errCod = fila?.FerrCod ?? 0;
        var errMsg = fila?.FerrMsg;

        if (errCod == 2)
        {
            await transaccion.RollbackAsync(ct).ConfigureAwait(false);
        }
        else
        {
            await transaccion.CommitAsync(ct).ConfigureAwait(false);
        }

        return Result.DesdeErrCod(errCod, errMsg, codigoUsuario);
    }

    private sealed record ResultadoRow
    {
        public int FerrCod { get; init; }

        public string? FerrMsg { get; init; }
    }
}
