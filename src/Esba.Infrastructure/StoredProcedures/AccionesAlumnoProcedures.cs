using System.Data.Common;
using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.StoredProcedures;

/// <summary>
/// Ejecución común de los SP de acciones sobre el alumno que escriben y devuelven
/// FERRCOD/FERRMSG pidiendo confirmación (patrón de dxBarButton29/30/43Click en
/// FrmEsba.pas): dentro de una transacción, FERRCOD=2 → rollback + Error; FERRCOD=1
/// → rollback + NeedsConfirmation si no se confirma, commit + Ok si se confirma;
/// FERRCOD=0 → commit. Los SP se CONSERVAN en esta fase (regla 🔴 §1.3).
/// </summary>
internal static class ProcedimientoConConfirmacion
{
    /// <summary>Fila de retorno fiel al RETURNS de los tres PSQL.</summary>
    private sealed record FilaResultado(int? FERRCOD, string? FERRMSG);

    public static async Task<Result<string>> EjecutarAsync(
        FbConnectionFactory connectionFactory,
        string sql,
        object parametros,
        bool confirmar,
        string mensajeConfirmado,
        CancellationToken ct)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using DbTransaction transaccion = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        var fila = await connection.QuerySingleAsync<FilaResultado>(new CommandDefinition(
            sql, parametros, transaction: transaccion, cancellationToken: ct)).ConfigureAwait(false);

        var mensaje = fila.FERRMSG?.TrimEnd();
        var resultado = Result.DesdeErrCod(fila.FERRCOD, mensaje, mensajeConfirmado);

        if (resultado.Status == OperationStatus.Error || (!confirmar && resultado.Status == OperationStatus.NeedsConfirmation))
        {
            await transaccion.RollbackAsync(ct).ConfigureAwait(false);
            return resultado;
        }

        await transaccion.CommitAsync(ct).ConfigureAwait(false);
        return resultado.Status == OperationStatus.NeedsConfirmation ? Result.Ok(mensajeConfirmado) : resultado;
    }
}

/// <summary>
/// SELECT FERRCOD, FERRMSG FROM XXX_COPIA_ALUMNO(@CarDde, @CarHta, @CodAlu, @CodUsu).
///
/// // TODO-migrar (prioridad baja): valida existencia en destino (altas/bajas) y hace
/// // un INSERT … SELECT de la fila de ALUMNOS con la carrera nueva, MATRIZ/CTT/FUSUWEB
/// // en NULL y USUARIO = quien copia. Portable a EF cuando se retire el SP.
/// </summary>
public sealed class CopiaAlumnoProcedure : ICopiaAlumnoProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public CopiaAlumnoProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<Result<string>> EjecutarAsync(CambioCarreraAlumnoParametros parametros, bool confirmar, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        return ProcedimientoConConfirmacion.EjecutarAsync(
            _connectionFactory,
            "SELECT FERRCOD, FERRMSG FROM XXX_COPIA_ALUMNO(@CarDde, @CarHta, @CodAlu, @CodUsu)",
            new
            {
                CarDde = parametros.CodigoCarreraOrigen,
                CarHta = parametros.CodigoCarreraDestino,
                CodAlu = parametros.CodigoAlumno,
                CodUsu = parametros.CodigoUsuario,
            },
            confirmar,
            $"Alumno copiado a la carrera {parametros.CodigoCarreraDestino}.",
            ct);
    }
}

/// <summary>
/// SELECT FERRCOD, FERRMSG FROM XXX_MUEVE_ALUMNO(@CarDde, @CarHta, @CodAlu, @CodUsu).
///
/// // TODO-migrar (prioridad baja): valida existencia en destino (altas/bajas) y que
/// // el alumno no tenga CURSADA ni ANALITIC en el origen; luego UPDATE ALUMNOS SET
/// // CARRE. Portable a EF cuando se retire el SP.
/// </summary>
public sealed class MueveAlumnoProcedure : IMueveAlumnoProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public MueveAlumnoProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<Result<string>> EjecutarAsync(CambioCarreraAlumnoParametros parametros, bool confirmar, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(parametros);

        return ProcedimientoConConfirmacion.EjecutarAsync(
            _connectionFactory,
            "SELECT FERRCOD, FERRMSG FROM XXX_MUEVE_ALUMNO(@CarDde, @CarHta, @CodAlu, @CodUsu)",
            new
            {
                CarDde = parametros.CodigoCarreraOrigen,
                CarHta = parametros.CodigoCarreraDestino,
                CodAlu = parametros.CodigoAlumno,
                CodUsu = parametros.CodigoUsuario,
            },
            confirmar,
            $"Alumno movido a la carrera {parametros.CodigoCarreraDestino}.",
            ct);
    }
}

/// <summary>
/// SELECT FERRCOD, FERRMSG FROM XXX_BORRA_ALUMNO(@Carre, @CodAlu, @Usuario).
///
/// // TODO-migrar (prioridad baja): verifica USUARIOS.SUPERV y borra ANALITIC,
/// // CURSADA, PERMEXA, ALUMNOS y FALTAS del alumno en la carrera. Portable a EF
/// // (cascada explícita) cuando se retire el SP.
/// </summary>
public sealed class BorraAlumnoProcedure : IBorraAlumnoProcedure
{
    private readonly FbConnectionFactory _connectionFactory;

    public BorraAlumnoProcedure(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<Result<string>> EjecutarAsync(
        string codigoCarrera, string codigoAlumno, int codigoUsuario, bool confirmar, CancellationToken ct) =>
        ProcedimientoConConfirmacion.EjecutarAsync(
            _connectionFactory,
            "SELECT FERRCOD, FERRMSG FROM XXX_BORRA_ALUMNO(@Carre, @CodAlu, @Usuario)",
            new { Carre = codigoCarrera, CodAlu = codigoAlumno, Usuario = codigoUsuario },
            confirmar,
            "Alumno eliminado con todo su historial.",
            ct);
}
