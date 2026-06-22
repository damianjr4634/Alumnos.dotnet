using Dapper;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.StoredProcedures;

namespace Esba.IntegrationTests.StoredProcedures;

/// <summary>
/// Equivalencia de los wrappers de permisos contra Firebird real
/// (PermisosPorUsuario.pas): YYY_SEGU_OPCIONES debe reflejar exactamente lo que
/// hay en BARRA_SEGU, y YYY_SEGU_GRABA debe dejar la tabla con el set que recibe.
/// No destructivo: graba el MISMO set que el usuario ya tiene (idempotente), así
/// que la base queda igual.
/// </summary>
[Trait("Category", "Integration")]
public class SeguPermisosEquivalenciaTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static FbConnectionFactory Factory() => new(ConnectionString);

    [Fact]
    public async Task Opciones_ReflejaLasFilasDeBarraSegu()
    {
        var ct = CancellationToken.None;
        var factory = Factory();
        var codigo = await UsuarioConPermisosAsync(factory, ct);
        if (codigo is null)
        {
            return; // ningún usuario con permisos cargados: nada que verificar.
        }

        var enTabla = await PermisosEnTablaAsync(factory, codigo.Value, ct);

        var opciones = await new SeguOpcionesProcedure(factory).ListarAsync(codigo.Value, ct);
        var habilitadas = opciones.Where(o => o.Habilitado).Select(o => o.Codigo)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Equal(enTabla, habilitadas);
        // Toda fila habilitada aparece una sola vez (sin duplicados entre los tres FOR del SP).
        Assert.Equal(opciones.Count(o => o.Habilitado), habilitadas.Count);
    }

    [Fact]
    public async Task Graba_ConElMismoSet_EsIdempotente()
    {
        var ct = CancellationToken.None;
        var factory = Factory();
        var codigo = await UsuarioConPermisosAsync(factory, ct);
        if (codigo is null)
        {
            return;
        }

        var antes = await PermisosEnTablaAsync(factory, codigo.Value, ct);

        var resultado = await new SeguGrabaProcedure(factory).GrabarAsync(codigo.Value, antes.ToList(), ct);
        Assert.Equal(OperationStatus.Ok, resultado.Status);

        var despues = await PermisosEnTablaAsync(factory, codigo.Value, ct);
        Assert.Equal(antes, despues);
    }

    /// <summary>Primer usuario con al menos un permiso en BARRA_SEGU; null si no hay.</summary>
    private static async Task<int?> UsuarioConPermisosAsync(FbConnectionFactory factory, CancellationToken ct)
    {
        await using var conn = await factory.CreateOpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<int?>(new CommandDefinition(
            "SELECT FIRST 1 CODUSU FROM BARRA_SEGU ORDER BY CODUSU", cancellationToken: ct));
    }

    private static async Task<HashSet<string>> PermisosEnTablaAsync(FbConnectionFactory factory, int codusu, CancellationToken ct)
    {
        await using var conn = await factory.CreateOpenConnectionAsync(ct);
        var filas = await conn.QueryAsync<string>(new CommandDefinition(
            "SELECT TRIM(BAROPC) FROM BARRA_SEGU WHERE CODUSU = @Cod", new { Cod = codusu }, cancellationToken: ct));
        return filas.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
