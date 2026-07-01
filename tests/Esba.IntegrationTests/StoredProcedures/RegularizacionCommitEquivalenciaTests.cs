using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia del <b>volcado</b> (commit) de la regularización contra el SP legacy
/// XXX_REGULARIZACION (Prompt 4.B). Para cada rama (TER, BAC) se corre el mismo caso por
/// dos caminos —el SP sobre "$$$CURSADA" y el seam C# <c>RegularizacionRepository.ConfirmarFilas*Async</c>
/// directo sobre CURSADA— cada uno en su propia transacción que se revierte, y se compara
/// el efecto sobre CURSADA / CURSADA_HST / ANALITIC. La base no se muta.
/// </summary>
/// <remarks>
/// Se excluyen de la comparación las columnas que asignan triggers/generadores
/// (CURSADA_HST.INDICE, ANALITIC.INDICE): se comparan por contenido, no por clave técnica.
/// Complementa a las equivalencias de <i>condición</i> (TERC / BAC+POSTVAL).
/// </remarks>
[Trait("Category", "Integration")]
public class RegularizacionCommitEquivalenciaTests
{
    private const int UsuarioPrueba = 9996;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    // Efecto observable del volcado sobre una cursada (comparado por igualdad de record).
    private sealed record Efecto(
        bool CursadaExiste,
        decimal? CurTpEva, decimal? CurTpEva2, decimal? CurRecup, decimal? CurRegular,
        decimal? CurFinal1, DateTime? CurFecha1, string? CurCondicion,
        decimal? CurTotHoras, decimal? CurInasist, decimal? CurJustif,
        int HstCount, string? HstCondicion, string? HstCondant,
        decimal? HstTpEva, decimal? HstTpEva2, decimal? HstRecup, string? HstMatriz,
        decimal? AnaNota, DateTime? AnaFecha, string? AnaCondicion,
        string? AnaMatriz, string? AnaInstituto, string? AnaCarac, string? AnaCuaAnio);

    private sealed record Clave(string Carre, string CodAlu, string CodMat);

    [Fact]
    public async Task Terciaria_Promociona_VuelcaIgualQueElSp()
    {
        // Cursada terciaria con materia PROMOCION='S' y fecha de cuatrimestre válida (sin analítico previo).
        var clave = new Clave("561/16", "DNI37951664", "02");
        const short cutuco = 0; // el volcado no usa CUTUCO; la clave es CARRE+COD_ALU+COD_MAT+CUA_ANIO.
        const string cuaAnio = "117";

        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        var efectoSp = await EfectoPorCaminoAsync(connection, clave, async (conn, tx) =>
        {
            await PoblarStagingTerciariaAsync(conn, tx, clave, cutuco, cuaAnio);
            await CorrerCommitSpAsync(conn, tx, clave.Carre);
        });

        var filaCs = new FilaRegularizacionResuelta
        {
            CodigoAlumno = clave.CodAlu,
            CodigoMateria = clave.CodMat,
            CuatrimestreAnio = cuaAnio,
            TpEva = 9m,
            TpEva2 = 9m,
            Recuperatorio = 0m,
            TotalHoras = 100,
            Inasistencias = 0,
            Justificadas = 0,
            NuevaCondicion = "PROMOCIONA",
            NotaAnalitico = 9m,
        };

        var efectoCs = await EfectoPorCaminoAsync(connection, clave, (conn, tx) =>
            RegularizacionRepository.ConfirmarFilasAsync(conn, tx, clave.Carre, UsuarioPrueba, [filaCs], CancellationToken.None));

        Assert.False(efectoSp.CursadaExiste);        // se movió a analítico
        Assert.NotNull(efectoSp.AnaNota);            // la prueba realmente ejerció el volcado
        Assert.Equal(efectoSp, efectoCs);
    }

    [Fact]
    public async Task Bachillerato_Regular_VuelcaIgualQueElSp()
    {
        var clave = new Clave("BAC", "DNI75788262", "07");
        const short cutuco = 241;
        const string cuaAnio = "113";
        var fecha = new DateTime(2024, 6, 30);

        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        var efectoSp = await EfectoPorCaminoAsync(connection, clave, async (conn, tx) =>
        {
            await PoblarStagingBachilleratoAsync(conn, tx, clave, cutuco, cuaAnio, "REGULAR", final1: 8m, fecha);
            await CorrerCommitSpAsync(conn, tx, clave.Carre);
        });

        var filaCs = FilaBac(clave, cuaAnio, "REGULAR", final1: 8m, fecha);
        var efectoCs = await EfectoPorCaminoAsync(connection, clave, (conn, tx) =>
            RegularizacionRepository.ConfirmarFilasBachilleratoAsync(conn, tx, clave.Carre, UsuarioPrueba, [filaCs], CancellationToken.None));

        Assert.False(efectoSp.CursadaExiste);
        Assert.NotNull(efectoSp.AnaNota);
        Assert.Equal("REGULAR", efectoSp.AnaCondicion);
        Assert.Equal(efectoSp, efectoCs);
    }

    [Fact]
    public async Task Bachillerato_NoAprobado_ActualizaCursadaIgualQueElSp()
    {
        var clave = new Clave("BAC", "DNI75788262", "07");
        const short cutuco = 241;
        const string cuaAnio = "113";
        var fecha = new DateTime(2024, 6, 30);

        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        var efectoSp = await EfectoPorCaminoAsync(connection, clave, async (conn, tx) =>
        {
            await PoblarStagingBachilleratoAsync(conn, tx, clave, cutuco, cuaAnio, "CURSANDO", final1: null, fecha);
            await CorrerCommitSpAsync(conn, tx, clave.Carre);
        });

        var filaCs = FilaBac(clave, cuaAnio, "CURSANDO", final1: null, fecha);
        var efectoCs = await EfectoPorCaminoAsync(connection, clave, (conn, tx) =>
            RegularizacionRepository.ConfirmarFilasBachilleratoAsync(conn, tx, clave.Carre, UsuarioPrueba, [filaCs], CancellationToken.None));

        Assert.True(efectoSp.CursadaExiste);          // no aprueba: la cursada sigue, sin analítico
        Assert.Null(efectoSp.AnaNota);
        Assert.Equal("CURSANDO", efectoSp.CurCondicion);
        Assert.Equal(efectoSp, efectoCs);
    }

    private static FilaRegularizacionBachilleratoResuelta FilaBac(
        Clave clave, string cuaAnio, string condicion, decimal? final1, DateTime fecha) => new()
    {
        CodigoAlumno = clave.CodAlu,
        CodigoMateria = clave.CodMat,
        CuatrimestreAnio = cuaAnio,
        TpEva = 8m,
        TpEva2 = 8m,
        Recuperatorio = 0m,
        NotaRegular = 0m,
        TotalHoras = 100,
        Inasistencias = 0,
        Justificadas = 0,
        Fecha = fecha,
        NuevaCondicion = condicion,
        NotaFinal = final1,
    };

    // Corre un camino de volcado en su propia transacción, captura el efecto y revierte.
    private static async Task<Efecto> EfectoPorCaminoAsync(
        FbConnection connection, Clave clave, Func<FbConnection, FbTransaction, Task> camino)
    {
        await using var tx = (FbTransaction)await connection.BeginTransactionAsync(CancellationToken.None);
        try
        {
            await camino(connection, tx);
            return await CapturarEfectoAsync(connection, tx, clave);
        }
        finally
        {
            await tx.RollbackAsync(CancellationToken.None);
        }
    }

    private static async Task PoblarStagingTerciariaAsync(
        FbConnection conn, FbTransaction tx, Clave clave, short cutuco, string cuaAnio)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM \"$$$CURSADA\" WHERE USUARIO = @U", new { U = UsuarioPrueba }, tx));
        await conn.ExecuteAsync(new CommandDefinition("""
            INSERT INTO "$$$CURSADA" (USUARIO, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, CONDICION,
                                      TP_EVA, TP_EVA2, RECUP, TOT_HORAS, INASIST, JUSTIF)
            VALUES (@U, @A, @M, @Cut, @Cua, 'PROMOCIONA', 9, 9, 0, 100, 0, 0)
            """,
            new { U = UsuarioPrueba, A = clave.CodAlu, M = clave.CodMat, Cut = cutuco, Cua = cuaAnio }, tx));
    }

    private static async Task PoblarStagingBachilleratoAsync(
        FbConnection conn, FbTransaction tx, Clave clave, short cutuco, string cuaAnio,
        string condicion, decimal? final1, DateTime fecha)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM \"$$$CURSADA\" WHERE USUARIO = @U", new { U = UsuarioPrueba }, tx));
        await conn.ExecuteAsync(new CommandDefinition("""
            INSERT INTO "$$$CURSADA" (USUARIO, COD_ALU, COD_MAT, CUTUCO, CUA_ANIO, CONDICION,
                                      TP_EVA, TP_EVA2, RECUP, REGULAR, TOT_HORAS, INASIST, JUSTIF, FINAL1, FECHA1)
            VALUES (@U, @A, @M, @Cut, @Cua, @Cond, 8, 8, 0, 0, 100, 0, 0, @Final1, @Fecha)
            """,
            new
            {
                U = UsuarioPrueba, A = clave.CodAlu, M = clave.CodMat, Cut = cutuco, Cua = cuaAnio,
                Cond = condicion, Final1 = final1, Fecha = fecha,
            }, tx));
    }

    // El SP es seleccionable: hay que traer la fila para que corra el cuerpo (procesa el usuario).
    private static async Task CorrerCommitSpAsync(FbConnection conn, FbTransaction tx, string carre)
    {
        _ = await conn.QueryFirstOrDefaultAsync<(int? FErrCod, string? FErrMsg)>(new CommandDefinition(
            "SELECT FERRCOD, FERRMSG FROM XXX_REGULARIZACION(@Carre, @U)",
            new { Carre = carre, U = UsuarioPrueba }, tx));
    }

    private static async Task<Efecto> CapturarEfectoAsync(FbConnection conn, FbTransaction tx, Clave clave)
    {
        var p = new { clave.Carre, clave.CodAlu, clave.CodMat };

        var cursada = await conn.QueryFirstOrDefaultAsync(new CommandDefinition("""
            SELECT TP_EVA, TP_EVA2, RECUP, REGULAR, FINAL1, FECHA1, TRIM(CONDICION) AS CONDICION,
                   TOT_HORAS, INASIST, JUSTIF
            FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
            """, p, tx));

        var hstCount = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM CURSADA_HST WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat", p, tx));

        // Fila más reciente de CURSADA_HST (la recién insertada tiene el mayor INDICE; el generador es monótono).
        var hst = await conn.QueryFirstOrDefaultAsync(new CommandDefinition("""
            SELECT FIRST 1 TRIM(CONDICION) AS CONDICION, TRIM(CONDANT) AS CONDANT,
                   TP_EVA, TP_EVA2, RECUP, TRIM(MATRIZ) AS MATRIZ
            FROM CURSADA_HST WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
            ORDER BY INDICE DESC
            """, p, tx));

        var ana = await conn.QueryFirstOrDefaultAsync(new CommandDefinition("""
            SELECT NOTA_MAT, FEC_FINAL, TRIM(CONDICION) AS CONDICION, TRIM(MATRIZ) AS MATRIZ,
                   TRIM("INSTITUT") AS INSTITUTO, TRIM(CARAC) AS CARAC, TRIM(CUA_ANIO) AS CUA_ANIO
            FROM ANALITIC WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
            """, p, tx));

        return new Efecto(
            CursadaExiste: cursada is not null,
            CurTpEva: (decimal?)cursada?.TP_EVA,
            CurTpEva2: (decimal?)cursada?.TP_EVA2,
            CurRecup: (decimal?)cursada?.RECUP,
            CurRegular: (decimal?)cursada?.REGULAR,
            CurFinal1: (decimal?)cursada?.FINAL1,
            CurFecha1: (DateTime?)cursada?.FECHA1,
            CurCondicion: (string?)cursada?.CONDICION,
            CurTotHoras: (decimal?)cursada?.TOT_HORAS,
            CurInasist: (decimal?)cursada?.INASIST,
            CurJustif: (decimal?)cursada?.JUSTIF,
            HstCount: hstCount,
            HstCondicion: (string?)hst?.CONDICION,
            HstCondant: (string?)hst?.CONDANT,
            HstTpEva: (decimal?)hst?.TP_EVA,
            HstTpEva2: (decimal?)hst?.TP_EVA2,
            HstRecup: (decimal?)hst?.RECUP,
            HstMatriz: (string?)hst?.MATRIZ,
            AnaNota: (decimal?)ana?.NOTA_MAT,
            AnaFecha: (DateTime?)ana?.FEC_FINAL,
            AnaCondicion: (string?)ana?.CONDICION,
            AnaMatriz: (string?)ana?.MATRIZ,
            AnaInstituto: (string?)ana?.INSTITUTO,
            AnaCarac: (string?)ana?.CARAC,
            AnaCuaAnio: (string?)ana?.CUA_ANIO);
    }
}
