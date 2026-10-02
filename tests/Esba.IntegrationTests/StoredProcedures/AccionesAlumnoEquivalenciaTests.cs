using Dapper;
using Esba.Application.Abstractions;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.StoredProcedures;
using FirebirdSql.Data.FirebirdClient;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia de los wrappers de XXX_COPIA_ALUMNO, XXX_MUEVE_ALUMNO y
/// XXX_BORRA_ALUMNO contra el SP real (Prompt 2.B/4.B). SQL legacy de referencia:
/// dxBarButton29/30/43Click de FrmEsba.pas. Ninguna prueba deja rastro: los caminos de
/// error (FERRCOD=2) no mutan y las previsualizaciones (FERRCOD=1) hacen rollback tanto
/// en el wrapper como en el SELECT directo de referencia, que corre en una transacción
/// que se revierte.
/// </summary>
[Trait("Category", "Integration")]
public class AccionesAlumnoEquivalenciaTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static Task<FbConnection> AbrirConexionAsync() => Factory.CreateOpenConnectionAsync(CancellationToken.None);

    private sealed record AlumnoRef(string Carre, string CodAlu);

    /// <summary>Un alumno con cursadas y una carrera destino donde no existe (activo o de baja).</summary>
    private static async Task<(AlumnoRef Alumno, string Destino)?> AlumnoConCursadaYDestinoLibreAsync(FbConnection connection)
    {
        return await connection.QueryFirstOrDefaultAsync<(string Carre, string CodAlu, string Destino)?>("""
            SELECT FIRST 1 TRIM(A.CARRE) AS Carre, A.COD_ALU AS CodAlu, TRIM(R.CARRE) AS Destino
            FROM ALUMNOS A
            JOIN CARRERA R ON R.CARRE <> A.CARRE AND R.DESACT = 'N'
            WHERE EXISTS (SELECT 1 FROM CURSADA C WHERE C.COD_ALU = A.COD_ALU AND C.CARRE = A.CARRE)
              AND NOT EXISTS (SELECT 1 FROM ALUMNOS B WHERE B.COD_ALU = A.COD_ALU AND B.CARRE = R.CARRE)
            ORDER BY A.CARRE, A.COD_ALU, R.CARRE
            """) is { } fila
            ? (new AlumnoRef(fila.Carre, fila.CodAlu), fila.Destino)
            : null;
    }

    private static async Task<int?> UsuarioAsync(FbConnection connection, bool supervisor) =>
        await connection.QueryFirstOrDefaultAsync<int?>(
            "SELECT FIRST 1 CODUSU FROM USUARIOS WHERE SUPERV = @Superv AND FECHA_BAJ IS NULL ORDER BY CODUSU",
            new { Superv = supervisor ? "S" : "N" });

    private static async Task<(int? FErrCod, string? FErrMsg)> SpDirectoConRollbackAsync(
        FbConnection connection, string sql, object parametros)
    {
        await using var transaccion = await connection.BeginTransactionAsync(CancellationToken.None);
        var fila = await connection.QuerySingleAsync<(int? FErrCod, string? FErrMsg)>(
            new CommandDefinition(sql, parametros, transaction: transaccion));
        await transaccion.RollbackAsync(CancellationToken.None);
        return fila;
    }

    private static Task<int> ContarAlumnosAsync(FbConnection connection, string carre, string codAlu) =>
        connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM ALUMNOS WHERE CARRE = @Carre AND COD_ALU = @CodAlu",
            new { Carre = carre, CodAlu = codAlu });

    [Fact]
    public async Task Copia_Previsualizar_CoincideConElSp_YNoDejaLaCopia()
    {
        await using var connection = await AbrirConexionAsync();
        var datos = await AlumnoConCursadaYDestinoLibreAsync(connection);
        Assert.True(datos is not null, "Se necesita un alumno con cursadas y una carrera donde no exista.");
        var (alumno, destino) = datos!.Value;
        var usuario = await UsuarioAsync(connection, supervisor: true) ?? 1;

        var parametros = new CambioCarreraAlumnoParametros
        {
            CodigoAlumno = alumno.CodAlu, CodigoCarreraOrigen = alumno.Carre, CodigoCarreraDestino = destino, CodigoUsuario = usuario,
        };

        var resultado = await new CopiaAlumnoProcedure(Factory).EjecutarAsync(parametros, confirmar: false, CancellationToken.None);

        var directo = await SpDirectoConRollbackAsync(connection,
            "SELECT FERRCOD, FERRMSG FROM XXX_COPIA_ALUMNO(@CarDde, @CarHta, @CodAlu, @CodUsu)",
            new { CarDde = alumno.Carre, CarHta = destino, CodAlu = alumno.CodAlu, CodUsu = usuario });

        Assert.Equal(1, directo.FErrCod);
        Assert.Equal(OperationStatus.NeedsConfirmation, resultado.Status);
        Assert.Equal(directo.FErrMsg?.TrimEnd(), resultado.Message);
        Assert.Contains("fue copiado a la carrera", resultado.Message, StringComparison.Ordinal);
        // La previsualización revirtió el INSERT del SP.
        Assert.Equal(0, await ContarAlumnosAsync(connection, destino, alumno.CodAlu));
    }

    [Fact]
    public async Task Copia_AlumnoYaExistenteEnDestino_WrapperYSpDevuelvenElMismoError()
    {
        await using var connection = await AbrirConexionAsync();
        var datos = await AlumnoConCursadaYDestinoLibreAsync(connection);
        Assert.True(datos is not null, "Se necesita un alumno con cursadas.");
        var (alumno, _) = datos!.Value;
        var usuario = await UsuarioAsync(connection, supervisor: true) ?? 1;

        // Copiarlo "a su propia carrera": ya existe en altas o en bajas → FERRCOD=2 sin mutar.
        var parametros = new CambioCarreraAlumnoParametros
        {
            CodigoAlumno = alumno.CodAlu, CodigoCarreraOrigen = alumno.Carre, CodigoCarreraDestino = alumno.Carre, CodigoUsuario = usuario,
        };

        var resultado = await new CopiaAlumnoProcedure(Factory).EjecutarAsync(parametros, confirmar: true, CancellationToken.None);

        var directo = await SpDirectoConRollbackAsync(connection,
            "SELECT FERRCOD, FERRMSG FROM XXX_COPIA_ALUMNO(@CarDde, @CarHta, @CodAlu, @CodUsu)",
            new { CarDde = alumno.Carre, CarHta = alumno.Carre, CodAlu = alumno.CodAlu, CodUsu = usuario });

        Assert.Equal(2, directo.FErrCod);
        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(directo.FErrMsg?.TrimEnd(), resultado.Message);
        Assert.Contains("ya existe", resultado.Message, StringComparison.Ordinal);
        Assert.Equal(1, await ContarAlumnosAsync(connection, alumno.Carre, alumno.CodAlu));
    }

    [Fact]
    public async Task Mover_AlumnoConCursadas_WrapperYSpDevuelvenElMismoError()
    {
        await using var connection = await AbrirConexionAsync();
        var datos = await AlumnoConCursadaYDestinoLibreAsync(connection);
        Assert.True(datos is not null, "Se necesita un alumno con cursadas y una carrera donde no exista.");
        var (alumno, destino) = datos!.Value;
        var usuario = await UsuarioAsync(connection, supervisor: true) ?? 1;

        var parametros = new CambioCarreraAlumnoParametros
        {
            CodigoAlumno = alumno.CodAlu, CodigoCarreraOrigen = alumno.Carre, CodigoCarreraDestino = destino, CodigoUsuario = usuario,
        };

        // Aun confirmando, el SP corta con FERRCOD=2 y el wrapper hace rollback.
        var resultado = await new MueveAlumnoProcedure(Factory).EjecutarAsync(parametros, confirmar: true, CancellationToken.None);

        var directo = await SpDirectoConRollbackAsync(connection,
            "SELECT FERRCOD, FERRMSG FROM XXX_MUEVE_ALUMNO(@CarDde, @CarHta, @CodAlu, @CodUsu)",
            new { CarDde = alumno.Carre, CarHta = destino, CodAlu = alumno.CodAlu, CodUsu = usuario });

        Assert.Equal(2, directo.FErrCod);
        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(directo.FErrMsg?.TrimEnd(), resultado.Message);
        Assert.Contains("materias cargadas", resultado.Message, StringComparison.Ordinal);
        Assert.Equal(1, await ContarAlumnosAsync(connection, alumno.Carre, alumno.CodAlu));
        Assert.Equal(0, await ContarAlumnosAsync(connection, destino, alumno.CodAlu));
    }

    [Fact]
    public async Task Borrar_UsuarioNoSupervisor_WrapperYSpDevuelvenElMismoError()
    {
        await using var connection = await AbrirConexionAsync();
        var datos = await AlumnoConCursadaYDestinoLibreAsync(connection);
        Assert.True(datos is not null, "Se necesita un alumno con cursadas.");
        var (alumno, _) = datos!.Value;
        var usuario = await UsuarioAsync(connection, supervisor: false);
        if (usuario is null)
        {
            return;   // sin usuarios no supervisores en la base: no hay camino que probar
        }

        var resultado = await new BorraAlumnoProcedure(Factory).EjecutarAsync(
            alumno.Carre, alumno.CodAlu, usuario.Value, confirmar: true, CancellationToken.None);

        var directo = await SpDirectoConRollbackAsync(connection,
            "SELECT FERRCOD, FERRMSG FROM XXX_BORRA_ALUMNO(@Carre, @CodAlu, @Usuario)",
            new { Carre = alumno.Carre, CodAlu = alumno.CodAlu, Usuario = usuario.Value });

        Assert.Equal(2, directo.FErrCod);
        Assert.Equal(OperationStatus.Error, resultado.Status);
        Assert.Equal(directo.FErrMsg?.TrimEnd(), resultado.Message);
        Assert.Equal(1, await ContarAlumnosAsync(connection, alumno.Carre, alumno.CodAlu));
    }

    [Fact]
    public async Task Borrar_Previsualizar_Supervisor_CoincideConElSp_YNoBorra()
    {
        await using var connection = await AbrirConexionAsync();
        var datos = await AlumnoConCursadaYDestinoLibreAsync(connection);
        Assert.True(datos is not null, "Se necesita un alumno con cursadas.");
        var (alumno, _) = datos!.Value;
        var usuario = await UsuarioAsync(connection, supervisor: true);
        Assert.True(usuario is not null, "Se necesita un usuario supervisor activo.");

        var cursadasAntes = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu",
            new { Carre = alumno.Carre, CodAlu = alumno.CodAlu });

        var resultado = await new BorraAlumnoProcedure(Factory).EjecutarAsync(
            alumno.Carre, alumno.CodAlu, usuario!.Value, confirmar: false, CancellationToken.None);

        var directo = await SpDirectoConRollbackAsync(connection,
            "SELECT FERRCOD, FERRMSG FROM XXX_BORRA_ALUMNO(@Carre, @CodAlu, @Usuario)",
            new { Carre = alumno.Carre, CodAlu = alumno.CodAlu, Usuario = usuario.Value });

        Assert.Equal(1, directo.FErrCod);
        Assert.Equal(OperationStatus.NeedsConfirmation, resultado.Status);
        Assert.Equal(directo.FErrMsg?.TrimEnd(), resultado.Message);
        // La previsualización revirtió los DELETE del SP: alumno y cursadas intactos.
        Assert.Equal(1, await ContarAlumnosAsync(connection, alumno.Carre, alumno.CodAlu));
        Assert.Equal(cursadasAntes, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu",
            new { Carre = alumno.Carre, CodAlu = alumno.CodAlu }));
    }
}
