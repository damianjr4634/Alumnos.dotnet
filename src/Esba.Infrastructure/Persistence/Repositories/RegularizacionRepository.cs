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
    // El CAST fija el tipo del parámetro (VARCHAR): sin él Firebird lo infiere más corto que el
    // código de 3 chars dentro del SUBSTRING y trunca al bindear.
    private const string SqlFechaPromocion = """
        SELECT IIF(SUBSTRING(CAST(@CuaAnio AS VARCHAR(10)) FROM 1 FOR 1) = '1', T.FHTAPRI, T.FHTASEG)
        FROM TBL_CUAT T
        WHERE T.FANIO = '20' || SUBSTRING(CAST(@CuaAnio AS VARCHAR(10)) FROM 2 FOR 2)
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

            // UPDATE CURSADA con las notas del cursado y la condición nueva.
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

            // Si aprueba directo, mover a histórico + analítico.
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

                // La rama TER de XXX_REGULARIZACION deja CURSADA_HST.CONDANT en NULL (a
                // diferencia de la rama BAC, que sí guarda la condición previa): se replica.
                await connection.ExecuteAsync(new CommandDefinition(
                    SqlMueveAHistorico,
                    new { clave.Carre, clave.CodAlu, clave.CodMat, CondAnt = (string?)null },
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

    // --- Bachillerato (rama BAC de XXX_REGULARIZACION) -------------------------------

    // La rama BAC persiste además REGULAR, FECHA1 y FINAL1 (no TP_EVA3/PROM).
    private const string SqlUpdateCursadaBac = """
        UPDATE CURSADA SET TP_EVA = @TpEva, TP_EVA2 = @TpEva2, RECUP = @Recup, CONDICION = @Condicion,
                           TOT_HORAS = @TotHoras, INASIST = @Inasist, JUSTIF = @Justif,
                           REGULAR = @Regular, FECHA1 = @Fecha, FINAL1 = @Final1, USUARIO = @Usuario
        WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat AND CUA_ANIO = @CuaAnio
        """;

    // CURSADA_HST de la rama BAC: sin FACTFIN* (INDICE lo pone el trigger CURSADA_HST_BIU0).
    private const string SqlMueveAHistoricoBac = """
        INSERT INTO CURSADA_HST (COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                 REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                 FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                 NREG, COLEGIO, "PLAN", A_C, DEFINE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                 FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, CONDANT)
        SELECT COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
               REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
               FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
               NREG, COLEGIO, "PLAN", A_C, DEFINE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
               FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, @CondAnt
        FROM CURSADA
        WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
        """;

    // La rama BAC inserta ANALITIC con la CONDICION real (REGULAR), nota FINAL1 y fecha FECHA1
    // (no la fecha de promoción de TBL_CUAT como terciarias). Sin FACTFIN.
    private const string SqlInsertaAnaliticoBac = """
        INSERT INTO ANALITIC (COD_ALU, APELLIDO, COD_MAT, CUA_ANIO, NOTA_MAT, FEC_FINAL, MATRIZ, CONDICION,
                              INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE, CARRE, COLEGIO, "PLAN", A_C, NREG, USUARIO)
        VALUES (@CodAlu, @Apellido, @CodMat, @CuaAnio, @NotaAna, @FechaAna, @Matriz, @Condicion,
                @Instituto, @Caracteristica, NULL, NULL, NULL, @Carre, NULL, NULL, NULL, NULL, @Usuario)
        """;

    public async Task<int> ConfirmarBachilleratoAsync(
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacionBachilleratoResuelta> filas,
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
            var procesadas = await ConfirmarFilasBachilleratoAsync(
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
    /// Coreografía del volcado de bachillerato (porta la rama BAC de XXX_REGULARIZACION)
    /// sobre una transacción provista. Seam interno del test de equivalencia (4.B), que la
    /// corre dentro de una transacción que luego revierte para no mutar la base.
    /// </summary>
    internal static async Task<int> ConfirmarFilasBachilleratoAsync(
        FbConnection connection,
        FbTransaction transaccion,
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacionBachilleratoResuelta> filas,
        CancellationToken ct)
    {
        var usuario = codigoUsuario.ToString(CultureInfo.InvariantCulture);

        var procesadas = 0;
        foreach (var fila in filas)
        {
            var clave = new { Carre = codigoCarrera, CodAlu = fila.CodigoAlumno, CodMat = fila.CodigoMateria };
            var cuaAnio = fila.CuatrimestreAnio.Replace("/", string.Empty, StringComparison.Ordinal).Trim();
            var esRegular = string.Equals(fila.NuevaCondicion, "REGULAR", StringComparison.Ordinal);

            // 1. Condición previa (para CURSADA_HST.CONDANT), antes del UPDATE.
            var condAnt = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
                SqlCondicionPrevia, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

            // 2. UPDATE CURSADA con las notas del cursado, la nota definitiva y la condición nueva.
            await connection.ExecuteAsync(new CommandDefinition(
                SqlUpdateCursadaBac,
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
                    Regular = fila.NotaRegular,
                    Fecha = fila.Fecha,
                    Final1 = fila.NotaFinal,
                    Condicion = fila.NuevaCondicion,
                    Usuario = usuario,
                },
                transaccion,
                cancellationToken: ct)).ConfigureAwait(false);

            // 3. Si la materia queda REGULAR, mover a histórico + analítico (nota FINAL1, fecha FECHA1).
            if (esRegular)
            {
                var datos = await connection.QueryFirstAsync<DatosAnalitico>(new CommandDefinition(
                    SqlDatosAnalitico, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

                await connection.ExecuteAsync(new CommandDefinition(
                    SqlMueveAHistoricoBac,
                    new { clave.Carre, clave.CodAlu, clave.CodMat, CondAnt = condAnt },
                    transaccion,
                    cancellationToken: ct)).ConfigureAwait(false);

                await connection.ExecuteAsync(new CommandDefinition(
                    SqlBorraCursada, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

                await connection.ExecuteAsync(new CommandDefinition(
                    SqlInsertaAnaliticoBac,
                    new
                    {
                        clave.CodAlu,
                        clave.CodMat,
                        clave.Carre,
                        datos.Apellido,
                        CuaAnio = cuaAnio,
                        NotaAna = fila.NotaFinal,
                        FechaAna = fila.Fecha,
                        datos.Matriz,
                        Condicion = fila.NuevaCondicion,
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

    // --- Secundario 333/650 (rama 333/650 de XXX_REGULARIZACION) ---------------------

    // Persiste los 3 trimestres, sus fechas y los exámenes de diciembre/marzo.
    private const string SqlUpdateCursada333 = """
        UPDATE CURSADA SET TP_EVA = @TpEva, TP_EVA2 = @TpEva2, TP_EVA3 = @TpEva3, CONDICION = @Condicion,
                           TOT_HORAS = @TotHoras, INASIST = @Inasist, JUSTIF = @Justif, FECHA1 = @Fecha,
                           NOTADIC = @NotaDic, NOTAMAR = @NotaMar, FECHDIC = @FechDic, FECHMAR = @FechMar,
                           FEC_EVA1 = @FecEva1, FEC_EVA2 = @FecEva2, FEC_EVA3 = @FecEva3, USUARIO = @Usuario
        WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat AND CUA_ANIO = @CuaAnio
        """;

    // CURSADA_HST de la rama 333/650: como la de BAC + diciembre/marzo (de CURSADA) y la nota
    // final NOTAFIN/NOTAFIN_FECHA (parámetros). INDICE lo pone el trigger CURSADA_HST_BIU0.
    private const string SqlMueveAHistorico333 = """
        INSERT INTO CURSADA_HST (COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                 REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                 FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                 NREG, COLEGIO, "PLAN", A_C, DEFINE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                 FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, CONDANT,
                                 NOTADIC, NOTAMAR, FECHDIC, FECHMAR, NOTAFIN, NOTAFIN_FECHA)
        SELECT COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
               REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
               FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
               NREG, COLEGIO, "PLAN", A_C, DEFINE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
               FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, @CondAnt,
               NOTADIC, NOTAMAR, FECHDIC, FECHMAR, @NotaFin, @NotaFinFecha
        FROM CURSADA
        WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
        """;

    public async Task<int> Confirmar333Async(
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacion333Resuelta> filas,
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
            var procesadas = await ConfirmarFilas333Async(
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
    /// Coreografía del volcado de secundario 333/650 (porta la rama 333/650 de
    /// XXX_REGULARIZACION) sobre una transacción provista. Seam interno del test de
    /// equivalencia (4.B), que la corre dentro de una transacción que luego revierte.
    /// </summary>
    internal static async Task<int> ConfirmarFilas333Async(
        FbConnection connection,
        FbTransaction transaccion,
        string codigoCarrera,
        int codigoUsuario,
        IReadOnlyList<FilaRegularizacion333Resuelta> filas,
        CancellationToken ct)
    {
        var usuario = codigoUsuario.ToString(CultureInfo.InvariantCulture);

        var procesadas = 0;
        foreach (var fila in filas)
        {
            var clave = new { Carre = codigoCarrera, CodAlu = fila.CodigoAlumno, CodMat = fila.CodigoMateria };
            var cuaAnio = fila.CuatrimestreAnio.Replace("/", string.Empty, StringComparison.Ordinal).Trim();
            var esRegular = string.Equals(fila.NuevaCondicion, "REGULAR", StringComparison.Ordinal);

            // Condición previa (la rama 333/650 sí la guarda en CURSADA_HST.CONDANT).
            var condAnt = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
                SqlCondicionPrevia, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

            await connection.ExecuteAsync(new CommandDefinition(
                SqlUpdateCursada333,
                new
                {
                    clave.Carre,
                    clave.CodAlu,
                    clave.CodMat,
                    CuaAnio = cuaAnio,
                    TpEva = fila.TpEva,
                    TpEva2 = fila.TpEva2,
                    TpEva3 = fila.TpEva3,
                    TotHoras = fila.TotalHoras,
                    Inasist = fila.Inasistencias,
                    Justif = fila.Justificadas,
                    Fecha = fila.Fecha,
                    NotaDic = fila.NotaDic,
                    NotaMar = fila.NotaMar,
                    FechDic = fila.FechDic,
                    FechMar = fila.FechMar,
                    FecEva1 = fila.FecEva1,
                    FecEva2 = fila.FecEva2,
                    FecEva3 = fila.FecEva3,
                    Condicion = fila.NuevaCondicion,
                    Usuario = usuario,
                },
                transaccion,
                cancellationToken: ct)).ConfigureAwait(false);

            if (esRegular)
            {
                var datos = await connection.QueryFirstAsync<DatosAnalitico>(new CommandDefinition(
                    SqlDatosAnalitico, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

                await connection.ExecuteAsync(new CommandDefinition(
                    SqlMueveAHistorico333,
                    new { clave.Carre, clave.CodAlu, clave.CodMat, CondAnt = condAnt, NotaFin = fila.NotaFinal, NotaFinFecha = fila.NotaFinalFecha },
                    transaccion,
                    cancellationToken: ct)).ConfigureAwait(false);

                await connection.ExecuteAsync(new CommandDefinition(
                    SqlBorraCursada, clave, transaccion, cancellationToken: ct)).ConfigureAwait(false);

                await connection.ExecuteAsync(new CommandDefinition(
                    SqlInsertaAnaliticoBac,
                    new
                    {
                        clave.CodAlu,
                        clave.CodMat,
                        clave.Carre,
                        datos.Apellido,
                        CuaAnio = cuaAnio,
                        NotaAna = fila.NotaFinal,
                        FechaAna = fila.NotaFinalFecha,
                        datos.Matriz,
                        Condicion = fila.NuevaCondicion,
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
