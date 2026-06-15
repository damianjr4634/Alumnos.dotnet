using Dapper;
using Esba.Application.Abstractions;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Academica;

/// <summary>
/// Roundtrip de la inscripción masiva contra Firebird real. Verifica el patrón de
/// dos fases del wrapper de XXX_INSC_CUAT_16032023: la previsualización
/// (confirmar=false) ejecuta el SP pero hace rollback, de modo que la cantidad de
/// CURSADA del alumno no cambia. No hace la fase de commit para no mutar datos.
/// </summary>
[Trait("Category", "Integration")]
public class InscripcionMasivaRoundtripTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static async Task<int> ContarCursadaAsync(string carre, string codAlu)
    {
        await using var ctx = CrearContexto();
        var conn = ctx.Database.GetDbConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CURSADA WHERE CARRE=@Carre AND COD_ALU=@CodAlu",
            new { Carre = carre, CodAlu = codAlu });
    }

    [Fact]
    public async Task Previsualizar_EjecutaElSpYHaceRollback_NoCambiaLaCursada()
    {
        var ct = CancellationToken.None;

        await using var ctx = CrearContexto();
        var alumno = await ctx.Alumnos.AsNoTracking()
            .Where(a => !a.Baja)
            .OrderBy(a => a.CodigoCarrera).ThenBy(a => a.Codigo)
            .FirstOrDefaultAsync(ct);
        if (alumno is null)
        {
            return; // base sin alumnos activos.
        }

        var carre = alumno.CodigoCarrera.Trim();
        var codAlu = alumno.Codigo.Trim();
        var antes = await ContarCursadaAsync(carre, codAlu);

        var procedimiento = new InscripcionMasivaCuatrimestreProcedure(new FbConnectionFactory(ConnectionString));
        var resultado = await procedimiento.EjecutarAsync(new InscripcionMasivaParametros
        {
            CodigoAlumno = codAlu,
            Curso = 111,                 // cuatrimestre 1, turno 1, comisión 1
            CodigoCarrera = carre,
            CuatrimestreAnio = "124",
            Instituto = null,
            Caracteristica = null,
            CodigoUsuario = 1,
        }, confirmar: false, ct);

        // Cualquiera sea el FERRCOD, la previsualización no debe persistir nada.
        Assert.NotNull(resultado);
        var despues = await ContarCursadaAsync(carre, codAlu);
        Assert.Equal(antes, despues); // el rollback dejó la base como estaba
    }
}
