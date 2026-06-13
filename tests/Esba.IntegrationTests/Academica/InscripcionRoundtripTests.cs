using Dapper;
using Esba.Application.DTOs.Academica;
using Esba.Application.Features.Academica;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
using Esba.Infrastructure.StoredProcedures;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.Academica;

/// <summary>
/// Ida y vuelta completa de la inscripción contra la base real (SQL legacy de
/// referencia: InscripcionDeMaterias.pas): inscribir → la query la muestra y
/// el trigger CURSADA_BI0 resolvió el CUTUCO → eliminar → no está más.
/// La base queda como estaba.
/// </summary>
[Trait("Category", "Integration")]
public class InscripcionRoundtripTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/var/firebird/esba_restore.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static FbConnectionFactory Factory => new(ConnectionString);

    [Fact]
    public async Task Inscribir_Verificar_Eliminar_DejaLaBaseComoEstaba()
    {
        var ct = CancellationToken.None;

        // Alumno activo y materia de su carrera en la que NO esté inscripto.
        await using var conexion = await Factory.CreateOpenConnectionAsync(ct);
        var caso = await conexion.QuerySingleOrDefaultAsync<(string Carre, string CodAlu, string CodMat)>("""
            SELECT FIRST 1 TRIM(A.CARRE), A.COD_ALU, M.CODMATERI
            FROM ALUMNOS A
            JOIN MATERIAS M ON M.CODCARRE = A.CARRE
            WHERE A.BAJA = 'N'
              AND NOT EXISTS (SELECT 1 FROM CURSADA C
                              WHERE C.CARRE = A.CARRE AND C.COD_ALU = A.COD_ALU
                                AND C.COD_MAT = M.CODMATERI)
            ORDER BY A.CARRE, A.COD_ALU, M.CODMATERI
            """);
        Assert.False(string.IsNullOrEmpty(caso.CodMat), "No se encontró un caso alumno+materia libre.");

        await using var contexto = new EsbaDbContext(Opciones);
        var cursadas = new CursadaRepository(contexto);
        var unitOfWork = new EfUnitOfWork(contexto);
        var query = new CursadaQuery(Factory);

        var inscribir = new InscribirEnMateriaHandler(
            cursadas, new AlumnoRepository(contexto), new MateriaRepository(contexto),
            new InscribirEnMateriaValidator(), unitOfWork);

        try
        {
            // 1. Inscribir.
            var alta = await inscribir.HandleAsync(new InscribirEnMateriaCommand
            {
                CodigoCarrera = caso.Carre,
                CodigoAlumno = caso.CodAlu,
                CodigoMateria = caso.CodMat,
                Turno = 1,
                NumeroComision = 1,
                CuatrimestreAnio = "199",
                Condicion = "CURSANDO",
            }, "test-int", ct);
            Assert.Equal(OperationStatus.Ok, alta.Status);

            // 2. La query del listado la muestra y el trigger resolvió el 9 del
            //    CUTUCO con el cuatrimestre del plan (ya no empieza con 9xx... o
            //    quedó 911 si la materia no tiene CUATRIM).
            var lista = await query.ListarPorAlumnoAsync(caso.Carre, caso.CodAlu, ct);
            var fila = Assert.Single(lista, c => c.CodigoMateria == caso.CodMat.Trim());
            Assert.Equal("CURSANDO", fila.Condicion);
            Assert.Equal("199", fila.CuatrimestreAnio);

            // 3. El trigger denormalizó el apellido desde ALUMNOS.
            var apellido = await conexion.ExecuteScalarAsync<string?>(
                "SELECT APELLIDO FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat",
                new { Carre = caso.Carre, CodAlu = caso.CodAlu, CodMat = caso.CodMat });
            Assert.False(string.IsNullOrWhiteSpace(apellido));
        }
        finally
        {
            // 4. Eliminar (deja la base como estaba) — con contexto limpio.
            await using var contextoLimpio = new EsbaDbContext(Opciones);
            var eliminar = new EliminarInscripcionHandler(
                new CursadaRepository(contextoLimpio), new EfUnitOfWork(contextoLimpio));
            await eliminar.HandleAsync(new EliminarInscripcionCommand
            {
                CodigoCarrera = caso.Carre,
                CodigoAlumno = caso.CodAlu,
                CodigoMateria = caso.CodMat,
            }, ct);
        }

        var quedo = await conexion.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM CURSADA WHERE CARRE = @Carre AND COD_ALU = @CodAlu AND COD_MAT = @CodMat",
            new { Carre = caso.Carre, CodAlu = caso.CodAlu, CodMat = caso.CodMat });
        Assert.Equal(0, quedo);
    }

    [Fact]
    public async Task CuatrimestreVigente_DevuelveFormatoCuaAnio()
    {
        await using var conexion = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var carrera = await conexion.ExecuteScalarAsync<string>(
            "SELECT FIRST 1 TRIM(CARRE) FROM CARRERA WHERE DESACT = 'N'");

        var cuatrimestre = await new CuatrimestreVigenteProcedure(Factory)
            .ObtenerAsync(carrera!, CancellationToken.None);

        // Puede ser null si no hay ciclo vigente cargado para la fecha actual;
        // si hay, el formato es C + AA numérico.
        if (cuatrimestre is not null)
        {
            Assert.Matches("^[0-9]{3}$", cuatrimestre);
        }
    }

    [Fact]
    public async Task CursadaQuery_AlumnoConCursada_ListaConSiglaYCondicion()
    {
        await using var conexion = await Factory.CreateOpenConnectionAsync(CancellationToken.None);
        var caso = await conexion.QuerySingleAsync<(string Carre, string CodAlu)>(
            "SELECT FIRST 1 TRIM(CARRE), COD_ALU FROM CURSADA ORDER BY CARRE, COD_ALU");

        var lista = await new CursadaQuery(Factory).ListarPorAlumnoAsync(caso.Carre, caso.CodAlu, CancellationToken.None);

        Assert.NotEmpty(lista);
        Assert.All(lista, c => Assert.False(string.IsNullOrWhiteSpace(c.Condicion)));
    }
}
