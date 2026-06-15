using Dapper;
using Esba.Application.Abstractions;
using Esba.Application.DTOs.Asistencias;
using Esba.Infrastructure.Persistence;

namespace Esba.Infrastructure.Persistence.Repositories;

/// <summary>
/// Escritura de FALTAS por Dapper (la clave única incluye COD_MAT nullable y el
/// patrón es reemplazo masivo, no change-tracking). Replica el delete+insert de
/// CargaInasistenciasComisionNuevo.GrabamesaClick en una transacción.
/// </summary>
public sealed class InasistenciasRepository : IInasistenciasRepository
{
    private const string SqlDelete = """
        DELETE FROM FALTAS
        WHERE CARRERA = @Carrera AND CUTUCO = @Cutuco
          AND ((@CodMat IS NULL AND COD_MAT IS NULL) OR COD_MAT = @CodMat)
          AND EXTRACT(YEAR FROM FECHA) = @Anio
        """;

    private const string SqlInsert = """
        INSERT INTO FALTAS (CARRERA, CODALU, CUTUCO, FECHA, CODFAL, CANFAL, COD_MAT, FDIACAR, USUARIO)
        VALUES (@Carrera, @CodAlu, @Cutuco, @Fecha, @CodFal, @CanFal, @CodMat, CURRENT_TIMESTAMP, @Usuario)
        """;

    private readonly FbConnectionFactory _connectionFactory;

    public InasistenciasRepository(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> ReemplazarFaltasComisionAsync(
        string codigoCarrera,
        short cutuco,
        string? codigoMateria,
        int anio,
        short? usuario,
        IReadOnlyList<FaltaInasistencia> faltas,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(faltas);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var transaccion = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                SqlDelete,
                new { Carrera = codigoCarrera, Cutuco = (int)cutuco, CodMat = codigoMateria, Anio = anio },
                transaction: transaccion,
                cancellationToken: ct)).ConfigureAwait(false);

            var insertados = 0;
            foreach (var falta in faltas)
            {
                insertados += await connection.ExecuteAsync(new CommandDefinition(
                    SqlInsert,
                    new
                    {
                        Carrera = codigoCarrera,
                        CodAlu = falta.CodigoAlumno,
                        Cutuco = (int)cutuco,
                        Fecha = falta.Fecha.ToDateTime(TimeOnly.MinValue),
                        CodFal = falta.CodigoFalta,
                        CanFal = falta.Cantidad,
                        CodMat = codigoMateria,
                        Usuario = usuario,
                    },
                    transaction: transaccion,
                    cancellationToken: ct)).ConfigureAwait(false);
            }

            await transaccion.CommitAsync(ct).ConfigureAwait(false);
            return insertados;
        }
        catch
        {
            await transaccion.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }
}
