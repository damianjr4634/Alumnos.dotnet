using Dapper;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Queries;

/// <summary>
/// Equivalencia de las queries de la resolución terciaria (hito 9.3d) contra SELECTs
/// directos en Firebird. Read-only.
/// </summary>
[Trait("Category", "Integration")]
public class EquivalenciaTerciariaQueryTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static async Task<(string Codigo, string Carrera)?> AlumnoTerciarioConEquivalenciaAsync(CancellationToken ct)
    {
        await using var ctx = CrearContexto();
        var fila = await ctx.Database.GetDbConnection().QueryFirstOrDefaultAsync<(string Cod, string Carre)>("""
            SELECT FIRST 1 TRIM(A.COD_ALU), TRIM(A.CARRE)
            FROM ANALITIC A JOIN CARRERA C ON C.CARRE = A.CARRE
            WHERE A.CONDICION = 'EQUIVALENCIA' AND TRIM(C.TIPO) = 'TER'
            ORDER BY A.CARRE, A.COD_ALU
            """);

        return fila.Cod is null ? null : (fila.Cod, fila.Carre);
    }

    [Fact]
    public async Task ObtenerEncabezado_TraeAlumnoYActas()
    {
        var ct = CancellationToken.None;
        if (await AlumnoTerciarioConEquivalenciaAsync(ct) is not { } alumno)
        {
            return;
        }

        var encabezado = await new EquivalenciaTerciariaQuery(Factory)
            .ObtenerEncabezadoAsync(alumno.Codigo, alumno.Carrera, ct);

        Assert.NotNull(encabezado);
        Assert.False(string.IsNullOrWhiteSpace(encabezado!.NombreAlumno));
        Assert.False(string.IsNullOrWhiteSpace(encabezado.ActasInternas));
        Assert.True(encabezado.AnioActual >= 2024);
    }

    [Fact]
    public async Task ListarMaterias_CuentaCoincideConSelectDirecto()
    {
        var ct = CancellationToken.None;
        if (await AlumnoTerciarioConEquivalenciaAsync(ct) is not { } alumno)
        {
            return;
        }

        var cuatrimestres = new[] { 1, 2 };
        await using var ctx = CrearContexto();
        var totalDirecto = await ctx.Database.GetDbConnection().ExecuteScalarAsync<int>("""
            SELECT COUNT(*)
            FROM ANALITIC A
            LEFT OUTER JOIN MATERIAS M ON A.COD_MAT = M.CODMATERI AND M.CODCARRE = A.CARRE
            WHERE A.COD_ALU = @A AND A.CARRE = @C AND A.CONDICION = 'EQUIVALENCIA' AND M.CUATRIM IN (1, 2)
            """, new { A = alumno.Codigo, C = alumno.Carrera });

        var materias = await new EquivalenciaTerciariaQuery(Factory)
            .ListarMateriasAsync(alumno.Codigo, alumno.Carrera, cuatrimestres, ct);

        Assert.Equal(totalDirecto, materias.Count);
        Assert.All(materias, m => Assert.Contains(m.Cuatrimestre, cuatrimestres));
    }
}
