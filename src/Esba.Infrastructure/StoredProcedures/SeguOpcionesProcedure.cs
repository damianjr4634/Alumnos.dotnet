using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Administracion;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT CADENA, HABILITA FROM YYY_SEGU_OPCIONES(@CodUsu). Lista las carreras
/// (CARRERA) y opciones de menú (BARRA_OPC), marcando HABILITA='S' las que el
/// usuario tiene en BARRA_SEGU. El wrapper separa el "CODIGO-Descripción" que
/// arma el SP.
///
/// // TODO-migrar (prioridad baja): el PSQL hace tres FOR/SUSPEND (habilitadas,
/// // carreras no habilitadas, opciones no habilitadas) — portarlo a una query
/// // C# es directo, pero de bajo riesgo, así que queda para la fase 5.
/// </summary>
public sealed class SeguOpcionesProcedure : ISeguOpcionesProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public SeguOpcionesProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PermisoOpcionDto>> ListarAsync(int codigoUsuario, CancellationToken ct)
    {
        const string sql = "SELECT CADENA, HABILITA FROM YYY_SEGU_OPCIONES(@CodUsu)";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<OpcionRow>(new CommandDefinition(
            sql, new { CodUsu = codigoUsuario }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.Select(Mapear).ToList();
    }

    private static PermisoOpcionDto Mapear(OpcionRow fila)
    {
        var cadena = (fila.Cadena ?? string.Empty).Trim();
        var guion = cadena.IndexOf('-', StringComparison.Ordinal);

        return new PermisoOpcionDto
        {
            // El SP arma "CODIGO-Descripción" y graba lo anterior al primer '-'.
            Codigo = guion >= 0 ? cadena[..guion] : cadena,
            Descripcion = guion >= 0 ? cadena[(guion + 1)..] : string.Empty,
            Habilitado = string.Equals(fila.Habilita?.Trim(), "S", StringComparison.OrdinalIgnoreCase),
        };
    }

    private sealed record OpcionRow
    {
        public string? Cadena { get; init; }

        public string? Habilita { get; init; }
    }
}
