using System.Globalization;
using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Esba.Infrastructure.Persistence.Repositories;

/// <summary>
/// Escritura de comisiones (COMARM). El guardado replica el flujo de
/// cargacomisiones.GrabaMateriaClick: insertar/actualizar y validar con
/// XXX_VAL_COMISION en la MISMA transacción (el SP lee la fila recién grabada),
/// con rollback si la validación falla. El post-SP necesita compartir la conexión
/// con EF Core, por eso la transacción se orquesta acá (§1.3, SP + EF compartiendo
/// conexión vía DbContext.Database.GetDbConnection()).
/// </summary>
public sealed class ComisionRepository : IComisionRepository
{
    private readonly EsbaDbContext _contexto;

    public ComisionRepository(EsbaDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Comision?> ObtenerAsync(
        string codigoCarrera, short cutuco, string codigoMateria, string cuatrimestreAnio, CancellationToken ct) =>
        _contexto.Comisiones.FirstOrDefaultAsync(
            c => c.CodigoCarrera == codigoCarrera && c.Cutuco == cutuco
                 && c.CodigoMateria == codigoMateria && c.CuatrimestreAnio == cuatrimestreAnio, ct);

    public async Task<Result<string>> GuardarYValidarAsync(Comision comision, bool esAlta, CancellationToken ct)
    {
        if (esAlta)
        {
            _contexto.Comisiones.Add(comision);
        }

        await using var transaccion = await _contexto.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            await _contexto.SaveChangesAsync(ct).ConfigureAwait(false);

            var validacion = await ValidarHorarioAsync(comision, transaccion, ct).ConfigureAwait(false);
            if (validacion.Status == OperationStatus.Error)
            {
                await transaccion.RollbackAsync(ct).ConfigureAwait(false);
                return validacion;
            }

            await transaccion.CommitAsync(ct).ConfigureAwait(false);
            return validacion;
        }
        catch
        {
            await transaccion.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }

    public void Eliminar(Comision comision) => _contexto.Comisiones.Remove(comision);

    /// <summary>
    /// Ejecuta XXX_VAL_COMISION sobre la conexión/transacción del DbContext (ve la
    /// fila recién grabada) y mapea FERRCOD/FERRMSG a Result (2 → Error).
    /// </summary>
    private async Task<Result<string>> ValidarHorarioAsync(Comision comision, IDbContextTransaction transaccion, CancellationToken ct)
    {
        const string sql = "SELECT FERRCOD, FERRMSG FROM XXX_VAL_COMISION(@Carre, @Cutuco, @CodMat, @CuaAnio)";

        var conexion = _contexto.Database.GetDbConnection();
        var fila = await conexion.QueryFirstOrDefaultAsync<(int FerrCod, string? FerrMsg)>(new CommandDefinition(
            sql,
            new
            {
                Carre = comision.CodigoCarrera,
                Cutuco = (int)comision.Cutuco,
                CodMat = comision.CodigoMateria,
                CuaAnio = int.Parse(comision.CuatrimestreAnio, CultureInfo.InvariantCulture),
            },
            transaction: transaccion.GetDbTransaction(),
            cancellationToken: ct)).ConfigureAwait(false);

        return Result.DesdeErrCod(fila.FerrCod, fila.FerrMsg, comision.CodigoMateria);
    }
}
