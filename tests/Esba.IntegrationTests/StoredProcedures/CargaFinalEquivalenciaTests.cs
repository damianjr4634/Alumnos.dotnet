using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia del volcado de finales portado a C# (CargaFinalRepository) contra
/// el SP legacy XXX_MESAS (Prompt 4.B): dada la MISMA carga resuelta (nota que
/// aprueba → condición FINAL), ambos caminos deben dejar la misma fila en
/// CURSADA_HST y en ANALITIC, y borrar la cursada. Tanto el SP (vía staging
/// "$$$PERMEXA") como el repo corren dentro de una transacción que se revierte:
/// la base no se muta. El cálculo de condición se cubre aparte con tests unitarios.
/// </summary>
[Trait("Category", "Integration")]
public class CargaFinalEquivalenciaTests
{
    private const int UsuarioPrueba = 9999;
    private const int MesaPrueba = 999999;

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    private sealed record CursadaRef(string Carre, string CodAlu, string CodMat, string? Matriz, string? Apellido, string? CondicionOriginal);

    private sealed record AnaliticoCap(decimal? NotaMat, DateTime? FecFinal, string? Condicion, string? Matriz, string? Institut, string? Carac, string? Factfin, string? CuaAnio);

    private sealed record HistCap(string? Condicion, string? Condant, decimal? Final1, DateTime? Fecha1, string? Factfin1, string? Matriz, string? CuaAnio);

    [Fact]
    public async Task FinalAprobado_RepositorioYSp_DejanElMismoHistoricoYAnalitico()
    {
        await using var connection = await Factory.CreateOpenConnectionAsync(CancellationToken.None);

        // Una cursada de una carrera terciaria en condición de rendir final.
        var cursada = await connection.QueryFirstOrDefaultAsync<CursadaRef>("""
            SELECT FIRST 1 TRIM(C.CARRE) AS Carre, C.COD_ALU AS CodAlu, TRIM(C.COD_MAT) AS CodMat,
                   TRIM(C.MATRIZ) AS Matriz, TRIM(C.APELLIDO) AS Apellido, TRIM(C.CONDICION) AS CondicionOriginal
            FROM CURSADA C
            INNER JOIN CARRERA R ON R.CARRE = C.CARRE
            WHERE R.TIPO = 'TER' AND TRIM(C.CONDICION) = 'REGULAR' AND COALESCE(TRIM(C.CUA_ANIO), '') <> ''
            """);

        Assert.True(cursada is not null, "Se necesita una cursada terciaria en condición REGULAR para la prueba.");

        var nota = 8m;
        var fecha = new DateTime(2026, 6, 1);
        const string acta = "TST";

        var porSp = await EjecutarPorSpAsync(connection, cursada!, nota, fecha, acta);
        var porRepo = await EjecutarPorRepositorioAsync(connection, cursada!, nota, fecha, acta);

        // CURSADA_HST: misma fila histórica.
        Assert.Equal(porSp.Hist.Condicion, porRepo.Hist.Condicion);
        Assert.Equal(porSp.Hist.Condant, porRepo.Hist.Condant);
        Assert.Equal(porSp.Hist.Final1, porRepo.Hist.Final1);
        Assert.Equal(porSp.Hist.Fecha1, porRepo.Hist.Fecha1);
        Assert.Equal(porSp.Hist.Factfin1, porRepo.Hist.Factfin1);
        Assert.Equal(porSp.Hist.Matriz, porRepo.Hist.Matriz);
        Assert.Equal(porSp.Hist.CuaAnio, porRepo.Hist.CuaAnio);

        // ANALITIC: misma fila de analítico.
        Assert.Equal(porSp.Analitico.NotaMat, porRepo.Analitico.NotaMat);
        Assert.Equal(porSp.Analitico.FecFinal, porRepo.Analitico.FecFinal);
        Assert.Equal(porSp.Analitico.Condicion, porRepo.Analitico.Condicion);
        Assert.Equal(porSp.Analitico.Matriz, porRepo.Analitico.Matriz);
        Assert.Equal(porSp.Analitico.Institut, porRepo.Analitico.Institut);
        Assert.Equal(porSp.Analitico.Carac, porRepo.Analitico.Carac);
        Assert.Equal(porSp.Analitico.Factfin, porRepo.Analitico.Factfin);
        Assert.Equal(porSp.Analitico.CuaAnio, porRepo.Analitico.CuaAnio);

        // Ambos caminos borran la cursada.
        Assert.Equal(0, porSp.CursadasRestantes);
        Assert.Equal(0, porRepo.CursadasRestantes);

        // La nota efectivamente pasó al analítico (no es una comparación de dos nulls).
        Assert.Equal(nota, porRepo.Analitico.NotaMat);
        Assert.Equal("FINAL", porRepo.Analitico.Condicion);
    }

    private sealed record Resultado(HistCap Hist, AnaliticoCap Analitico, int CursadasRestantes);

    private static async Task<Resultado> EjecutarPorSpAsync(
        FbConnection connection, CursadaRef c, decimal nota, DateTime fecha, string acta)
    {
        await using var tx = (FbTransaction)await connection.BeginTransactionAsync(CancellationToken.None);

        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM \"$$$PERMEXA\" WHERE USUARIO = @Usuario", new { Usuario = UsuarioPrueba }, tx));

        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO "$$$PERMEXA" (PERM_EXA, FINAL1, FECHA1, MATRIZ, COD_ALU, MESA, CARRE, USUARIO,
                                      APELLIDO, NOM_APE, CONDICION, COD_MAT, CONDIANT, FACTFIN1)
            VALUES (1, @Nota, @Fecha, @Matriz, @CodAlu, @Mesa, @Carre, @Usuario,
                    @Apellido, @Apellido, 'FINAL', @CodMat, @CondAnt, @Acta)
            """,
            new
            {
                Nota = nota,
                Fecha = fecha,
                c.Matriz,
                CodAlu = c.CodAlu,
                Mesa = MesaPrueba,
                Carre = c.Carre,
                Usuario = UsuarioPrueba,
                Apellido = c.Apellido,
                CodMat = c.CodMat,
                CondAnt = c.CondicionOriginal,
                Acta = acta,
            },
            tx));

        // El UPDATE/movimiento lo hace el SP leyendo el staging. Es un procedure
        // seleccionable: hay que traer la fila (QueryFirst) para que corra el cuerpo.
        _ = await connection.QueryFirstOrDefaultAsync<(int? FErrCod, string? FErrMsg)>(new CommandDefinition(
            "SELECT FERRCOD, FERRMSG FROM XXX_MESAS(@Usuario)",
            new { Usuario = UsuarioPrueba.ToString(System.Globalization.CultureInfo.InvariantCulture) }, tx));

        var resultado = await CapturarAsync(connection, tx, c);
        await tx.RollbackAsync(CancellationToken.None);
        return resultado;
    }

    private static async Task<Resultado> EjecutarPorRepositorioAsync(
        FbConnection connection, CursadaRef c, decimal nota, DateTime fecha, string acta)
    {
        await using var tx = (FbTransaction)await connection.BeginTransactionAsync(CancellationToken.None);

        var fila = new FilaCargaFinalResuelta
        {
            CodigoAlumno = c.CodAlu,
            CodigoMateria = c.CodMat,
            EsTerciaria = true,
            Nota1 = nota,
            Fecha1 = DateOnly.FromDateTime(fecha),
            Acta1 = acta,
            NuevaCondicion = "FINAL",
            NotaAnalitico = nota,
            FechaAnalitico = DateOnly.FromDateTime(fecha),
            ActaAnalitico = acta,
        };

        await CargaFinalRepository.ConfirmarFilasAsync(
            connection, tx, c.Carre, MesaPrueba, UsuarioPrueba, consumirPermiso: true, [fila], CancellationToken.None);

        var resultado = await CapturarAsync(connection, tx, c);
        await tx.RollbackAsync(CancellationToken.None);
        return resultado;
    }

    private static async Task<Resultado> CapturarAsync(FbConnection connection, FbTransaction tx, CursadaRef c)
    {
        var clave = new { Carre = c.Carre, CodAlu = c.CodAlu, CodMat = c.CodMat };

        var hist = await connection.QueryFirstAsync<HistCap>(new CommandDefinition("""
            SELECT FIRST 1 TRIM(CONDICION) AS Condicion, TRIM(CONDANT) AS Condant, FINAL1 AS Final1,
                   FECHA1 AS Fecha1, TRIM(FACTFIN1) AS Factfin1, TRIM(MATRIZ) AS Matriz, TRIM(CUA_ANIO) AS CuaAnio
            FROM CURSADA_HST
            WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
            ORDER BY INDICE DESC
            """, clave, tx));

        var analitico = await connection.QueryFirstAsync<AnaliticoCap>(new CommandDefinition("""
            SELECT FIRST 1 NOTA_MAT AS NotaMat, FEC_FINAL AS FecFinal, TRIM(CONDICION) AS Condicion,
                   TRIM(MATRIZ) AS Matriz, TRIM(INSTITUT) AS Institut, TRIM(CARAC) AS Carac,
                   TRIM(FACTFIN) AS Factfin, TRIM(CUA_ANIO) AS CuaAnio
            FROM ANALITIC
            WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat
            ORDER BY INDICE DESC
            """, clave, tx));

        var restantes = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat",
            clave, tx));

        return new Resultado(hist, analitico, restantes);
    }
}
