using System.Globalization;
using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.Infrastructure.Persistence.Repositories;

/// <summary>
/// Volcado de las notas de final de una mesa. Porta el SP XXX_MESAS a C#
/// (decisión 2026-06-26: se elimina el staging "$$$PERMEXA"). Toda la mesa se
/// procesa en una sola transacción: por fila, UPDATE CURSADA y, si el final
/// aprueba, mover la cursada a CURSADA_HST + insertar en ANALITIC + borrar la
/// cursada y el permiso consumido de PERMEXA. El orden replica el del SP: el
/// DELETE de CURSADA va antes del INSERT de ANALITIC porque el trigger
/// ANALITIC_BI0 prohíbe que una materia esté en CURSADA y ANALITIC a la vez.
///
/// Los SP de identidad/auditoría se conservan: INDICE de CURSADA_HST lo pone su
/// trigger (por eso no se inserta), y LOG_CURSADA/LOG_ANALITIC los escriben sus
/// triggers AFTER. La paridad con el SP se cubre con un test de equivalencia.
/// </summary>
public sealed class CargaFinalRepository : ICargaFinalRepository
{
    private const string SqlCondicionPrevia =
        "SELECT FIRST 1 TRIM(CONDICION) FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat";

    private const string SqlUpdateTerciaria = """
        UPDATE CURSADA SET FINAL1 = @Nota1, FINAL2 = @Nota2, FINAL3 = @Nota3,
                           FECHA1 = @Fecha1, FECHA2 = @Fecha2, FECHA3 = @Fecha3,
                           FACTFIN1 = @Acta1, FACTFIN2 = @Acta2, FACTFIN3 = @Acta3,
                           CONDICION = @Condicion, USUARIO = @Usuario
        WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
        """;

    private const string SqlUpdateBachiller = """
        UPDATE CURSADA SET FINAL1 = @Nota1, FECHA1 = @Fecha1,
                           CONDICION = @Condicion, USUARIO = @Usuario
        WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
        """;

    // Datos de CURSADA/CARRERA que el INSERT de ANALITIC necesita y que hay que
    // leer ANTES de borrar la cursada (el SP los toma del staging/CARRERA).
    private const string SqlDatosAnalitico = """
        SELECT TRIM(C.APELLIDO) AS Apellido, TRIM(C.CUA_ANIO) AS CuatrimestreAnio, TRIM(C.MATRIZ) AS Matriz,
               (SELECT TRIM(R.INSTITUT) FROM CARRERA R WHERE R.CARRE = @Carre) AS Instituto,
               (SELECT TRIM(R.CARACT)   FROM CARRERA R WHERE R.CARRE = @Carre) AS Caracteristica
        FROM CURSADA C
        WHERE C.CARRE = @Carre AND C.COD_ALU = @CodAlu AND C.COD_MAT = @CodMat
        """;

    private const string SqlBorraPermiso =
        "DELETE FROM PERMEXA WHERE MESA = @Mesa AND CARRE = @Carre AND COD_ALU = @CodAlu";

    // INSERT ... SELECT con las mismas columnas que XXX_MESAS (INDICE lo pone el
    // trigger CURSADA_HST_BIU0, por eso no va en la lista). CONDANT = condición previa.
    private const string SqlMueveAHistorico = """
        INSERT INTO CURSADA_HST (COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                 REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                 FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                 NREG, COLEGIO, "PLAN", A_C, DEFINE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                 FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, CONDANT, FACTFIN1, FACTFIN2, FACTFIN3)
        SELECT COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
               REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
               FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
               NREG, COLEGIO, "PLAN", A_C, DEFINE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
               FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, @CondAnt, FACTFIN1, FACTFIN2, FACTFIN3
        FROM CURSADA
        WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
        """;

    private const string SqlBorraCursada =
        "DELETE FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat";

    private const string SqlInsertaAnalitico = """
        INSERT INTO ANALITIC (COD_ALU, APELLIDO, COD_MAT, CUA_ANIO, NOTA_MAT, FEC_FINAL, MATRIZ, CONDICION,
                              INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE, CARRE, COLEGIO, "PLAN", A_C, NREG, USUARIO, FACTFIN)
        VALUES (@CodAlu, @Apellido, @CodMat, @CuaAnio, @NotaAna, @FechaAna, @Matriz, @Condicion,
                @Instituto, @Caracteristica, NULL, NULL, NULL, @Carre, NULL, NULL, NULL, NULL, @Usuario, @ActaAna)
        """;

    private readonly FbConnectionFactory _connectionFactory;

    public CargaFinalRepository(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> ConfirmarAsync(
        string codigoCarrera,
        int mesa,
        int codigoUsuario,
        IReadOnlyList<FilaCargaFinalResuelta> filas,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filas);
        if (filas.Count == 0)
        {
            return 0;
        }

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var transaccion = (FbTransaction)await connection.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            var procesadas = await ConfirmarFilasAsync(
                connection, transaccion, codigoCarrera, mesa, codigoUsuario, filas, ct).ConfigureAwait(false);
            await transaccion.CommitAsync(ct).ConfigureAwait(false);
            return procesadas;
        }
        catch
        {
            await transaccion.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Coreografía del volcado (porta XXX_MESAS) sobre una transacción provista.
    /// La expone como seam interno el test de equivalencia (4.B), que la ejecuta
    /// dentro de una transacción que luego revierte para no mutar la base.
    /// </summary>
    internal static async Task<int> ConfirmarFilasAsync(
        FbConnection connection,
        FbTransaction transaccion,
        string codigoCarrera,
        int mesa,
        int codigoUsuario,
        IReadOnlyList<FilaCargaFinalResuelta> filas,
        CancellationToken ct)
    {
        var usuario = codigoUsuario.ToString(CultureInfo.InvariantCulture);

        var procesadas = 0;
        foreach (var fila in filas)
        {
            var clave = new { Carre = codigoCarrera, CodAlu = fila.CodigoAlumno, CodMat = fila.CodigoMateria };

                // 1. Condición previa (para CURSADA_HST.CONDANT), antes del UPDATE.
                var condAnt = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
                    SqlCondicionPrevia, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

                // 2. UPDATE CURSADA con las notas y la condición nueva.
                var sqlUpdate = fila.EsTerciaria ? SqlUpdateTerciaria : SqlUpdateBachiller;
                await connection.ExecuteAsync(new CommandDefinition(
                    sqlUpdate,
                    new
                    {
                        clave.Carre,
                        clave.CodAlu,
                        clave.CodMat,
                        Nota1 = fila.Nota1,
                        Nota2 = fila.Nota2,
                        Nota3 = fila.Nota3,
                        Fecha1 = fila.Fecha1,
                        Fecha2 = fila.Fecha2,
                        Fecha3 = fila.Fecha3,
                        Acta1 = fila.Acta1,
                        Acta2 = fila.Acta2,
                        Acta3 = fila.Acta3,
                        Condicion = fila.NuevaCondicion,
                        Usuario = usuario,
                    },
                    transaccion,
                    cancellationToken: ct)).ConfigureAwait(false);

                // 3. Si el final aprueba, mover a histórico + analítico.
                if (fila.NotaAnalitico is not null)
                {
                    var datos = await connection.QueryFirstAsync<DatosAnalitico>(new CommandDefinition(
                        SqlDatosAnalitico, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

                    await connection.ExecuteAsync(new CommandDefinition(
                        SqlBorraPermiso,
                        new { Mesa = mesa, clave.Carre, clave.CodAlu },
                        transaccion,
                        cancellationToken: ct)).ConfigureAwait(false);

                    await connection.ExecuteAsync(new CommandDefinition(
                        SqlMueveAHistorico,
                        new { clave.Carre, clave.CodAlu, clave.CodMat, CondAnt = condAnt },
                        transaccion,
                        cancellationToken: ct)).ConfigureAwait(false);

                    await connection.ExecuteAsync(new CommandDefinition(
                        SqlBorraCursada, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

                    await connection.ExecuteAsync(new CommandDefinition(
                        SqlInsertaAnalitico,
                        new
                        {
                            clave.CodAlu,
                            clave.CodMat,
                            clave.Carre,
                            datos.Apellido,
                            CuaAnio = datos.CuatrimestreAnio,
                            NotaAna = fila.NotaAnalitico,
                            FechaAna = fila.FechaAnalitico,
                            datos.Matriz,
                            Condicion = fila.NuevaCondicion,
                            datos.Instituto,
                            datos.Caracteristica,
                            ActaAna = fila.ActaAnalitico,
                            Usuario = usuario,
                        },
                        transaccion,
                        cancellationToken: ct)).ConfigureAwait(false);
                }

            procesadas++;
        }

        return procesadas;
    }

    private sealed record DatosAnalitico(
        string? Apellido,
        string? CuatrimestreAnio,
        string? Matriz,
        string? Instituto,
        string? Caracteristica);
}
