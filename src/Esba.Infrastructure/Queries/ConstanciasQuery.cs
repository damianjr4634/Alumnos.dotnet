using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Queries;

public sealed class ConstanciasQuery : IConstanciasQuery
{
    private readonly FbConnectionFactory _connectionFactory;

    public ConstanciasQuery(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CarreraConstanciaDto?> ObtenerDatosCarreraAsync(string codigoCarrera, CancellationToken ct)
    {
        // STRCuatAnio del legacy: ' AÑO; ' para 333/650, ' CUAT.; ' para el resto.
        const string sql = """
            SELECT TRIM(DESCARRE)  AS Nombre,
                   TRIM(DESCARRE2) AS NombreAlternativo,
                   TRIM(DURACION)  AS Duracion,
                   TRIM(RESOLUCION) AS Resolucion,
                   TRIM(RECTOR)    AS Rector,
                   TRIM(SECRETARIA) AS Secretaria,
                   TRIM(IDIOMA)    AS Idioma,
                   TRIM(INSTITUT)  AS Instituto,
                   TRIM(CARACT)    AS Caracteristica,
                   IIF(TRIM(CARRE) IN ('333', '650'), TRUE, FALSE) AS EsCarreraPorAnio
            FROM CARRERA
            WHERE TRIM(CARRE) = @Carre
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await connection.QueryFirstOrDefaultAsync<CarreraConstanciaDto>(
            new CommandDefinition(sql, new { Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);
    }
}
