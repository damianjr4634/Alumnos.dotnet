using Dapper;
using Esba.Application.DTOs.Examenes;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Examenes;

/// <summary>
/// Tests de permisos de examen contra Firebird real: wrapper XXX_MATERIAS_FINALES
/// (read-only) y roundtrip de PERMEXA (alta/listado/baja) con un alumno de prueba.
/// </summary>
[Trait("Category", "Integration")]
public class PermisosExamenRoundtripTests
{
    private const string AlumnoPrueba = "TESTPERM001";

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static async Task LimpiarAsync(string carrera)
    {
        await using var ctx = CrearContexto();
        var conn = ctx.Database.GetDbConnection();
        await conn.ExecuteAsync("DELETE FROM PERMEXA WHERE COD_ALU=@A AND CARRE=@C",
            new { A = AlumnoPrueba, C = carrera });
    }

    [Fact]
    public async Task MateriasFinales_ParaUnAlumnoReal_NoLanza()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();
        var alumno = await ctx.Alumnos.AsNoTracking()
            .Where(a => !a.Baja)
            .OrderBy(a => a.CodigoCarrera).ThenBy(a => a.Codigo)
            .FirstOrDefaultAsync(ct);
        if (alumno is null)
        {
            return;
        }

        var finales = await new MateriasFinalesProcedure(Factory)
            .ListarAsync(alumno.Codigo.Trim(), alumno.CodigoCarrera.Trim(), ct);

        Assert.All(finales, f => Assert.False(string.IsNullOrWhiteSpace(f.CodigoMateria)));
    }

    [Fact]
    public async Task Permexa_AltaListadoBaja()
    {
        var ct = CancellationToken.None;

        // Una mesa real (con materia) para usar sus datos.
        await using var conn = await Factory.CreateOpenConnectionAsync(ct);
        var mesa = await conn.QueryFirstOrDefaultAsync<(string Carre, int Mesa, string CodMat)?>(
            "SELECT FIRST 1 TRIM(CARRE), MESA, TRIM(COD_MAT) FROM MESAS WHERE COD_MAT IS NOT NULL ORDER BY CARRE, MESA");
        if (mesa is null)
        {
            return; // sin mesas cargadas.
        }

        var (carrera, numeroMesa, codMat) = mesa.Value;
        await LimpiarAsync(carrera);
        var repo = new PermisosExamenRepository(Factory);

        try
        {
            await repo.InsertarAsync(new CrearPermisoExamenCommand
            {
                CodigoCarrera = carrera,
                CodigoAlumno = AlumnoPrueba,
                Apellido = "TEST",
                Mesa = numeroMesa,
                Cutuco = 111,
                CodigoMateria = codMat,
                CodigoUsuario = 1,
            }, ct);

            Assert.True(await repo.ExisteAsync(carrera, AlumnoPrueba, numeroMesa, codMat, ct));

            var lista = await repo.ListarPorAlumnoAsync(carrera, AlumnoPrueba, ct);
            Assert.Contains(lista, p => p.CodigoMateria == codMat && p.Mesa == numeroMesa);

            var borrados = await repo.EliminarAsync(carrera, AlumnoPrueba, codMat, ct);
            Assert.True(borrados > 0);
        }
        finally
        {
            await LimpiarAsync(carrera);
        }
    }
}
