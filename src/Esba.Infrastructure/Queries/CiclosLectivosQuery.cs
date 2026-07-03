using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Academica;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

public sealed class CiclosLectivosQuery : ICiclosLectivosQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public CiclosLectivosQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CicloCuatrimestralDto>> ListarCuatrimestralesAsync(CancellationToken ct)
    {
        // Legacy: SELECT FANIO, FDDEPRI, FHTAPRI, FDDESEG, FHTASEG FROM TBL_CUAT (CargadeTrimestres.pas).
        const string sql = """
            SELECT FANIO   AS Anio,
                   FDDEPRI AS PrimerCuatrimestreDesde,
                   FHTAPRI AS PrimerCuatrimestreHasta,
                   FDDESEG AS SegundoCuatrimestreDesde,
                   FHTASEG AS SegundoCuatrimestreHasta
            FROM TBL_CUAT
            ORDER BY FANIO DESC
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<CicloCuatrimestralDto>(
            new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }

    public async Task<IReadOnlyList<CicloTrimestralDto>> ListarTrimestralesAsync(CancellationToken ct)
    {
        const string sql = """
            SELECT FANIO   AS Anio,
                   FDDEPRI AS PrimerTrimestreDesde,
                   FHTAPRI AS PrimerTrimestreHasta,
                   FDDESEG AS SegundoTrimestreDesde,
                   FHTASEG AS SegundoTrimestreHasta,
                   FDDETER AS TercerTrimestreDesde,
                   FHTATER AS TercerTrimestreHasta
            FROM TBL_TRIM
            ORDER BY FANIO DESC
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<CicloTrimestralDto>(
            new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
