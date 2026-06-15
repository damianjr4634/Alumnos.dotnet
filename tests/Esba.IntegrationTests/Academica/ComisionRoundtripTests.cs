using Dapper;
using Esba.Domain.Common;
using Esba.Domain.Entities;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Academica;

/// <summary>
/// Roundtrip del guardado de comisiones contra Firebird real: ejercita la
/// transacción de ComisionRepository.GuardarYValidarAsync + el SP XXX_VAL_COMISION
/// (la parte que los tests unitarios con mocks no cubren). Usa CUTUCO/CUA_ANIO de
/// prueba improbables y limpia siempre lo que crea.
/// </summary>
[Trait("Category", "Integration")]
public class ComisionRoundtripTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static EsbaDbContext CrearContexto() => new(Opciones);

    /// <summary>Una materia real con cuatrimestre 1..9 (para construir un CUTUCO válido).</summary>
    private static async Task<Materia?> MateriaConCuatrimestreAsync(EsbaDbContext ctx, CancellationToken ct) =>
        await ctx.Materias.AsNoTracking()
            .Where(m => m.Cuatrimestre != null && m.Cuatrimestre >= 1 && m.Cuatrimestre <= 9)
            .OrderBy(m => m.CodigoCarrera).ThenBy(m => m.Codigo)
            .FirstOrDefaultAsync(ct);

    private static async Task BorrarComisionAsync(string carre, short cutuco, string codMat, string cuaAnio)
    {
        await using var ctx = CrearContexto();
        var conn = ctx.Database.GetDbConnection();
        await conn.ExecuteAsync(
            "DELETE FROM COMARM WHERE CARRE=@Carre AND CUTUCO=@Cutuco AND COD_MAT=@CodMat AND CUA_ANIO=@CuaAnio",
            new { Carre = carre, Cutuco = (int)cutuco, CodMat = codMat, CuaAnio = cuaAnio });
    }

    [Fact]
    public async Task GuardarYValidar_AltaSinSuperposicion_CommiteaYPersiste()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();
        var materia = await MateriaConCuatrimestreAsync(ctx, ct);
        if (materia is null)
        {
            return; // sin materias con cuatrimestre cargado; nada que verificar.
        }

        var cuatrim = materia.Cuatrimestre!.Value;
        var cutuco = (short)((cuatrim * 100) + 99);   // 1er dígito = cuatrimestre → pasa la validación
        var cuaAnio = $"{cuatrim}99";
        await BorrarComisionAsync(materia.CodigoCarrera, cutuco, materia.Codigo, cuaAnio);

        try
        {
            var comision = NuevaComision(materia, cutuco, cuaAnio);
            var resultado = await new ComisionRepository(CrearContexto())
                .GuardarYValidarAsync(comision, esAlta: true, ct);

            Assert.Equal(OperationStatus.Ok, resultado.Status);

            await using var verificacion = CrearContexto();
            var persistida = await verificacion.Comisiones.AsNoTracking().FirstOrDefaultAsync(
                c => c.CodigoCarrera == materia.CodigoCarrera && c.Cutuco == cutuco
                     && c.CodigoMateria == materia.Codigo && c.CuatrimestreAnio == cuaAnio, ct);
            Assert.NotNull(persistida);
        }
        finally
        {
            await BorrarComisionAsync(materia.CodigoCarrera, cutuco, materia.Codigo, cuaAnio);
        }
    }

    [Fact]
    public async Task GuardarYValidar_CuatrimestreNoCoincideConLaMateria_RollbackYNoPersiste()
    {
        var ct = CancellationToken.None;
        await using var ctx = CrearContexto();
        var materia = await MateriaConCuatrimestreAsync(ctx, ct);
        if (materia is null)
        {
            return;
        }

        var cuatrim = materia.Cuatrimestre!.Value;
        // 1er dígito del CUTUCO distinto del cuatrimestre de la materia → FERRCOD=2.
        var cutucoMalo = (short)((((cuatrim % 9) + 1) * 100) + 99);
        var cuaAnio = $"{cuatrim}99";
        await BorrarComisionAsync(materia.CodigoCarrera, cutucoMalo, materia.Codigo, cuaAnio);

        try
        {
            var comision = NuevaComision(materia, cutucoMalo, cuaAnio);
            var resultado = await new ComisionRepository(CrearContexto())
                .GuardarYValidarAsync(comision, esAlta: true, ct);

            Assert.Equal(OperationStatus.Error, resultado.Status);

            // El rollback dejó la base sin la fila.
            await using var verificacion = CrearContexto();
            var persistida = await verificacion.Comisiones.AsNoTracking().FirstOrDefaultAsync(
                c => c.CodigoCarrera == materia.CodigoCarrera && c.Cutuco == cutucoMalo
                     && c.CodigoMateria == materia.Codigo && c.CuatrimestreAnio == cuaAnio, ct);
            Assert.Null(persistida);
        }
        finally
        {
            await BorrarComisionAsync(materia.CodigoCarrera, cutucoMalo, materia.Codigo, cuaAnio);
        }
    }

    private static Comision NuevaComision(Materia materia, short cutuco, string cuaAnio) => new()
    {
        CodigoCarrera = materia.CodigoCarrera,
        Cutuco = cutuco,
        CodigoMateria = materia.Codigo,
        CuatrimestreAnio = cuaAnio,
        CodigoProfesor = null,
        TitularSuplente = "T",
        Usuario = "test",
        // Sin horario (todo BLANCO): la validación de superposición no aplica.
        Dia1 = "BLANCO",
        Bloque1 = "BLANCO",
        Dia2 = "BLANCO",
        Bloque2 = "BLANCO",
        Dia3 = "BLANCO",
        Bloque3 = "BLANCO",
    };
}
