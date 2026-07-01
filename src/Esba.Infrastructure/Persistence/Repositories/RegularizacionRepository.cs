using System.Globalization;
using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.Infrastructure.Persistence.Repositories;

/// <summary>
/// Volcado de la regularización terciaria. Porta la rama TER del SP XXX_REGULARIZACION
/// a C# (decisión hito 15: se elimina el staging "$$$CURSADA"). Por fila, en una sola
/// transacción: UPDATE CURSADA con las notas y la condición; y si la materia se aprueba
/// directo (PROMOCIONA/FINAL) mueve la cursada a CURSADA_HST, la borra e inserta en
/// ANALITIC. El DELETE de CURSADA va antes del INSERT de ANALITIC (trigger ANALITIC_BI0).
/// </summary>
/// <remarks>
/// INDICE de CURSADA_HST lo pone su trigger (no se inserta); LOG_CURSADA/LOG_ANALITIC
/// los escriben sus triggers AFTER. La paridad con el SP se cubre con un test de
/// equivalencia (4.B).
/// </remarks>
public sealed class RegularizacionRepository : IRegularizacionRepository
{
    private const string SqlCondicionPrevia =
        "SELECT FIRST 1 TRIM(CONDICION) FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat";

    private const string SqlUpdateCursada = """
        UPDATE CURSADA SET TP_EVA = @TpEva, TP_EVA2 = @TpEva2, RECUP = @Recup, CONDICION = @Condicion,
                           TOT_HORAS = @TotHoras, INASIST = @Inasist, JUSTIF = @Justif, USUARIO = @Usuario
        WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat AND CUA_ANIO = @CuaAnio
        """;

    // Fecha de promoción del cuatrimestre (XXX_FECHA_REGULARIZACION / rama TER de XXX_REGULARIZACION).
    private const string SqlFechaPromocion = """
        SELECT IIF(SUBSTRING(@CuaAnio FROM 1 FOR 1) = '1', T.FHTAPRI, T.FHTASEG)
        FROM TBL_CUAT T
        WHERE T.FANIO = '20' || SUBSTRING(@CuaAnio FROM 2 FOR 2)
        """;

    // Apellido de CURSADA, matriz de ALUMNOS e instituto/característica de CARRERA (como el SP).
    private const string SqlDatosAnalitico = """
        SELECT TRIM(C.APELLIDO) AS Apellido,
               (SELECT TRIM(A.MATRIZ)   FROM ALUMNOS A WHERE A.CARRE = @Carre AND A.COD_ALU = @CodAlu) AS Matriz,
               (SELECT TRIM(R.INSTITUT) FROM CARRERA R WHERE R.CARRE = @Carre) AS Instituto,
               (SELECT TRIM(R.CARACT)   FROM CARRERA R WHERE R.CARRE = @Carre) AS Caracteristica
        FROM CURSADA C
        WHERE C.CARRE = @Carre AND C.COD_ALU = @CodAlu AND C.COD_MAT = @CodMat
        """;

    // INSERT ... SELECT con las mismas columnas que la rama TER del SP (INDICE lo pone el
    // trigger CURSADA_HST_BIU0, por eso no va). CONDANT = condición previa.
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

    // La rama TER inserta ANALITIC con CONDICION fija 'FINAL' (tanto PROMOCIONA como FINAL).
    private const string SqlInsertaAnalitico = """
        INSERT INTO ANALITIC (COD_ALU, APELLIDO, COD_MAT, CUA_ANIO, NOTA_MAT, FEC_FINAL, MATRIZ, CONDICION,
                              INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE, CARRE, COLEGIO, "PLAN", A_C, NREG, USUARIO, FACTFIN)
        VALUES (@CodAlu, @Apellido, @CodMat, @CuaAnio, @NotaAna, @FechaAna, @Matriz, 'FINAL',
                @Instituto, @Caracteristica, NULL, NULL, NULL, @Carre, NULL, NULL, NULL, NULL, @Usuario, NULL)
        """;

    private readonly FbConnectionFactory _connectionFactory;

    public RegularizacionRepository(FbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> ConfirmarTerciariaAsync(
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacionResuelta> filas,
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
                connection, transaccion, codigoCarrera, codigoUsuario, filas, ct).ConfigureAwait(false);
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
    /// Coreografía del volcado (porta la rama TER de XXX_REGULARIZACION) sobre una
    /// transacción provista. La expone como seam interno el test de equivalencia (4.B),
    /// que la corre dentro de una transacción que luego revierte para no mutar la base.
    /// </summary>
    internal static async Task<int> ConfirmarFilasAsync(
        FbConnection connection,
        FbTransaction transaccion,
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacionResuelta> filas,
        CancellationToken ct)
    {
        var usuario = codigoUsuario.ToString(CultureInfo.InvariantCulture);

        var procesadas = 0;
        foreach (var fila in filas)
        {
            var clave = new { Carre = codigoCarrera, CodAlu = fila.CodigoAlumno, CodMat = fila.CodigoMateria };
            var cuaAnio = fila.CuatrimestreAnio.Replace("/", string.Empty, StringComparison.Ordinal).Trim();

            // 1. Condición previa (para CURSADA_HST.CONDANT), antes del UPDATE.
            var condAnt = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
                SqlCondicionPrevia, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

            // 2. UPDATE CURSADA con las notas del cursado y la condición nueva.
            await connection.ExecuteAsync(new CommandDefinition(
                SqlUpdateCursada,
                new
                {
                    clave.Carre,
                    clave.CodAlu,
                    clave.CodMat,
                    CuaAnio = cuaAnio,
                    TpEva = fila.TpEva,
                    TpEva2 = fila.TpEva2,
                    Recup = fila.Recuperatorio,
                    TotHoras = fila.TotalHoras,
                    Inasist = fila.Inasistencias,
                    Justif = fila.Justificadas,
                    Condicion = fila.NuevaCondicion,
                    Usuario = usuario,
                },
                transaccion,
                cancellationToken: ct)).ConfigureAwait(false);

            // 3. Si aprueba directo, mover a histórico + analítico.
            if (fila.NotaAnalitico is not null)
            {
                var fechaPromocion = await connection.ExecuteScalarAsync<DateTime?>(new CommandDefinition(
                    SqlFechaPromocion, new { CuaAnio = cuaAnio }, transaccion, cancellationToken: ct)).ConfigureAwait(false);
                if (fechaPromocion is null)
                {
                    throw new InvalidOperationException(
                        $"Falta la fecha de promoción del cuatrimestre {cuaAnio}. Revise TBL_CUAT para el año 20{cuaAnio[1..]}.");
                }

                var datos = await connection.QueryFirstAsync<DatosAnalitico>(new CommandDefinition(
                    SqlDatosAnalitico, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

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
                        CuaAnio = cuaAnio,
                        NotaAna = fila.NotaAnalitico,
                        FechaAna = fechaPromocion,
                        datos.Matriz,
                        datos.Instituto,
                        datos.Caracteristica,
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
        string? Matriz,
        string? Instituto,
        string? Caracteristica);
}
