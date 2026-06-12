using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.StoredProcedures;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia del wrapper contra el SP real (Prompt 2.B/4.B): misma entrada,
/// la salida del wrapper debe coincidir con el SELECT directo a
/// XXX_CAMBIA_DNI_LM. SQL legacy de referencia: CambioCodAlu_LM.pas.
/// El camino de error no muta datos (repetible); el camino exitoso revierte
/// con el propio SP al valor original.
/// </summary>
[Trait("Category", "Integration")]
public class CambioDniLibroMatrizEquivalenciaTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/var/firebird/esba_restore.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static CambioDniLibroMatrizProcedure CrearWrapper() => new(Factory);

    private static Task<FbConnection> AbrirConexionAsync() =>
        Factory.CreateOpenConnectionAsync(CancellationToken.None);

    private sealed record AlumnoRef(string Carre, string CodAlu, string Matriz);

    private static async Task<(AlumnoRef Uno, AlumnoRef Otro)> DosAlumnosConMatrizAsync(FbConnection connection)
    {
        var alumnos = (await connection.QueryAsync<AlumnoRef>("""
            SELECT FIRST 2 TRIM(CARRE) AS Carre, COD_ALU AS CodAlu, TRIM(MATRIZ) AS Matriz
            FROM ALUMNOS
            WHERE CARRE = (SELECT FIRST 1 CARRE FROM ALUMNOS
                           WHERE COALESCE(TRIM(MATRIZ), '') <> ''
                           GROUP BY CARRE HAVING COUNT(*) >= 2)
              AND COALESCE(TRIM(MATRIZ), '') <> ''
            ORDER BY COD_ALU
            """)).AsList();

        Assert.True(alumnos.Count == 2, "Se necesitan dos alumnos con matriz en la misma carrera.");
        return (alumnos[0], alumnos[1]);
    }

    [Fact]
    public async Task MatrizOcupada_WrapperYSpDirecto_DevuelvenElMismoError()
    {
        await using var connection = await AbrirConexionAsync();
        var (uno, otro) = await DosAlumnosConMatrizAsync(connection);

        // Intento asignarle a "uno" la matriz de "otro": el SP no muta nada.
        var parametros = new CambioDniLibroMatrizParametros
        {
            CodigoAlumno = uno.CodAlu,
            CodigoCarrera = uno.Carre,
            Tipo = 'L',
            NuevoLibroMatriz = otro.Matriz,
        };

        var resultado = await CrearWrapper().EjecutarAsync(parametros, CancellationToken.None);

        var directo = await connection.QuerySingleAsync<(int? FErrCod, string? FErrMsg)>(
            "SELECT FERRCOD, FERRMSG FROM XXX_CAMBIA_DNI_LM(@CodAlu, @Lm, @NuevoCodAlu, 'L', @Carre)",
            new { CodAlu = uno.CodAlu, Lm = otro.Matriz, NuevoCodAlu = (string?)null, Carre = uno.Carre });

        // Campo por campo: misma semántica que el SELECT directo.
        Assert.Equal(2, directo.FErrCod);
        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(directo.FErrMsg?.TrimEnd(), resultado.Message);
        Assert.Contains("pertenece a otro alumno", resultado.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CambioDeMatrizLibre_AplicaYPropagaa_LuegoRevierte()
    {
        await using var connection = await AbrirConexionAsync();
        var (alumno, _) = await DosAlumnosConMatrizAsync(connection);

        // Matriz libre en la carrera (CHAR(5)).
        var ocupadas = (await connection.QueryAsync<string>(
            "SELECT DISTINCT TRIM(MATRIZ) FROM ALUMNOS WHERE CARRE = @Carre AND MATRIZ IS NOT NULL",
            new { alumno.Carre })).ToHashSet(StringComparer.Ordinal);
        var libre = Enumerable.Range(0, 999).Select(i => $"ZZ{i:000}").First(m => !ocupadas.Contains(m));

        var wrapper = CrearWrapper();
        try
        {
            var resultado = await wrapper.EjecutarAsync(new CambioDniLibroMatrizParametros
            {
                CodigoAlumno = alumno.CodAlu,
                CodigoCarrera = alumno.Carre,
                Tipo = 'L',
                NuevoLibroMatriz = libre,
            }, CancellationToken.None);

            // FERRCOD=0 con mensaje → Warning (aviso informativo, completó), §1.3.
            Assert.Equal(OperationStatus.Warning, resultado.Status);
            Assert.True(resultado.IsSuccess);
            Assert.Equal("Se cambio el libro matriz", resultado.Message);

            var matrizActual = await connection.ExecuteScalarAsync<string>(
                "SELECT TRIM(MATRIZ) FROM ALUMNOS WHERE CARRE = @Carre AND COD_ALU = @CodAlu",
                new { alumno.Carre, CodAlu = alumno.CodAlu });
            Assert.Equal(libre, matrizActual);
        }
        finally
        {
            // Reversión con el propio SP: la base queda como estaba.
            await wrapper.EjecutarAsync(new CambioDniLibroMatrizParametros
            {
                CodigoAlumno = alumno.CodAlu,
                CodigoCarrera = alumno.Carre,
                Tipo = 'L',
                NuevoLibroMatriz = alumno.Matriz,
            }, CancellationToken.None);
        }

        var matrizFinal = await connection.ExecuteScalarAsync<string>(
            "SELECT TRIM(MATRIZ) FROM ALUMNOS WHERE CARRE = @Carre AND COD_ALU = @CodAlu",
            new { alumno.Carre, CodAlu = alumno.CodAlu });
        Assert.Equal(alumno.Matriz, matrizFinal);
    }

    [Theory]
    [InlineData('X')]
    [InlineData(' ')]
    public async Task TipoInvalido_LanzaSinTocarLaBase(char tipo)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CrearWrapper().EjecutarAsync(
            new CambioDniLibroMatrizParametros
            {
                CodigoAlumno = "DNI00000001",
                CodigoCarrera = "XXX",
                Tipo = tipo,
                NuevoLibroMatriz = "00000",
            }, CancellationToken.None));
    }

    [Fact]
    public async Task TipoLSinMatrizNueva_Lanza()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CrearWrapper().EjecutarAsync(
            new CambioDniLibroMatrizParametros
            {
                CodigoAlumno = "DNI00000001",
                CodigoCarrera = "XXX",
                Tipo = 'L',
            }, CancellationToken.None));
    }
}
