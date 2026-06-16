using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Certificados;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT ... FROM XXX_CONSTANCIA_TERCIARIA(@CodAlu, @Carre).
///
/// // TODO-migrar (prioridad alta): arma el analítico/constancia de materias
/// // cruzando MATERIAS con CURSADA y ANALITIC, resolviendo condición, colores y
/// // permisos web. Para la carrera 'ADM' delega en XXX_CONSTANCIA_ADM. Es lógica
/// // de negocio central del analítico; portarla requiere el modelo de condiciones.
/// </summary>
public sealed class ConstanciaMateriasProcedure : IConstanciaMateriasProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public ConstanciaMateriasProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ConstanciaMateriaDto>> ListarAsync(string codigoAlumno, string codigoCarrera, CancellationToken ct)
    {
        const string sql = """
            SELECT CUATRIM        AS Cuatrimestre,
                   TRIM(CODMAT)   AS CodigoMateria,
                   TRIM(SIGLA)    AS Sigla,
                   TRIM(DESCRIPCI) AS Descripcion,
                   TRIM(CONDICION) AS Condicion,
                   TRIM(CUAANIO)  AS CuatrimestreAnio,
                   NOTA           AS Nota,
                   FECHA          AS Fecha,
                   TRIM(INSTITUTO) AS Instituto,
                   TRIM(CARACT)   AS Caracteristica,
                   TRIM(ANUAL)    AS Anual
            FROM XXX_CONSTANCIA_TERCIARIA(@CodAlu, @Carre)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<ConstanciaMateriaDto>(new CommandDefinition(
            sql, new { CodAlu = codigoAlumno, Carre = codigoCarrera }, cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
