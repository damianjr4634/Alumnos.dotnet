using System.Globalization;
using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// SELECT FERRCOD, FERRMSG FROM XXX_INSC_CUAT_16032023(...). Inscripción masiva
/// por cuatrimestre con el patrón de dos fases sin transacción de larga vida
/// (deuda hito 6): ejecuta el SP en una transacción propia y, según
/// <c>confirmar</c>, hace rollback (preview) o commit (confirmada).
///
/// // TODO-migrar (prioridad media): el PSQL recorre las materias del cuatrimestre
/// // (MATERIAS.CUATRIM = 1er dígito del curso, ESTADO&lt;&gt;'B'), valida cada una con
/// // XXX_INSC_VALMAT e inserta en CURSADA; FERRCOD=1 es el override de supervisor
/// // (insertó pese a errores, requiere confirmación), FERRCOD=2 error duro.
/// </summary>
public sealed class InscripcionMasivaCuatrimestreProcedure : IInscripcionMasivaCuatrimestreProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public InscripcionMasivaCuatrimestreProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<string>> EjecutarAsync(InscripcionMasivaParametros parametros, bool confirmar, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        const string sql = """
            SELECT FERRCOD AS FerrCod, FERRMSG AS FerrMsg
            FROM XXX_INSC_CUAT_16032023(@CodAlu, @Curso, @Carre, @CuaAnio, @Instituto, @Carac, @Usuario)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var transaccion = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        var fila = await connection.QueryFirstOrDefaultAsync<ResultadoRow>(new CommandDefinition(
            sql,
            new
            {
                CodAlu = parametros.CodigoAlumno,
                Curso = (int)parametros.Curso,
                Carre = parametros.CodigoCarrera,
                CuaAnio = int.Parse(parametros.CuatrimestreAnio, CultureInfo.InvariantCulture),
                Instituto = parametros.Instituto,
                Carac = parametros.Caracteristica,
                Usuario = parametros.CodigoUsuario,
            },
            transaction: transaccion,
            cancellationToken: ct)).ConfigureAwait(false);

        var errCod = fila?.FerrCod ?? 0;
        var errMsg = fila?.FerrMsg;

        // Commit solo si se confirma y no hubo error duro; en cualquier otro caso
        // (preview, o error) se descarta lo insertado.
        if (confirmar && errCod != 2)
        {
            await transaccion.CommitAsync(ct).ConfigureAwait(false);
        }
        else
        {
            await transaccion.RollbackAsync(ct).ConfigureAwait(false);
        }

        return Result.DesdeErrCod(errCod, errMsg, errMsg ?? string.Empty);
    }

    private sealed record ResultadoRow
    {
        public int FerrCod { get; init; }

        public string? FerrMsg { get; init; }
    }
}
