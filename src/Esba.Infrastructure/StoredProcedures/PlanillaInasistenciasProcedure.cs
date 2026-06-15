using System.Globalization;
using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT ... FROM XXX_FALTAS_IMPRESI(@Carre,@Cutuco,@CuaAnio,@ReinPri,@ReinSeg,@Libre).
///
/// // TODO-migrar (prioridad baja): el PSQL suma las faltas por tipo (TBL_FALTAS
/// // FTIPO/FJUSTIF) en el rango del trimestre (TBL_TRIM), cuenta ACTAS_DISC y
/// // clasifica al alumno según los umbrales. Es lógica de reporte pura.
/// </summary>
public sealed class PlanillaInasistenciasProcedure : IPlanillaInasistenciasProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public PlanillaInasistenciasProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PlanillaReincorporacionDto>> ListarAsync(
        string codigoCarrera,
        string cutuco,
        string cuatrimestreAnio,
        decimal reincorporacionPrimera,
        decimal reincorporacionSegunda,
        decimal libre,
        CancellationToken ct)
    {
        const string sql = """
            SELECT TRIM(COD_ALU) AS CodigoAlumno,
                   TRIM(NOMBRE)  AS Nombre,
                   CUTUCO        AS Cutuco,
                   INAJUS        AS Justificadas,
                   INAINJUS      AS Injustificadas,
                   INATAR        AS Tardanzas,
                   INAEDFIS      AS EducacionFisica,
                   INATOT        AS Total,
                   TRIM(PRISEG)  AS Estado,
                   FECHA         AS Fecha,
                   ACTDISC       AS ActasDisciplina
            FROM XXX_FALTAS_IMPRESI(@Carre, @Cutuco, @CuaAnio, @ReinPri, @ReinSeg, @Libre)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        var filas = await connection.QueryAsync<PlanillaReincorporacionDto>(new CommandDefinition(
            sql,
            new
            {
                Carre = codigoCarrera,
                Cutuco = cutuco,
                CuaAnio = cuatrimestreAnio,
                ReinPri = reincorporacionPrimera.ToString(CultureInfo.InvariantCulture),
                ReinSeg = reincorporacionSegunda.ToString(CultureInfo.InvariantCulture),
                Libre = libre.ToString(CultureInfo.InvariantCulture),
            },
            cancellationToken: ct)).ConfigureAwait(false);

        return filas.AsList();
    }
}
