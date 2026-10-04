using Dapper;
using Esba.Application.DTOs.AreaDocente;
using Esba.Application.Features.AreaDocente;
using Esba.Application.Validators;
using Esba.Domain.Common;
using Esba.Domain.Enums;
using Esba.Infrastructure.Persistence;
using Esba.Infrastructure.Persistence.Repositories;
using Esba.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;

namespace Esba.IntegrationTests.AreaDocente;

/// <summary>
/// Roundtrip de la precarga por mesa contra Firebird real (DOC_CARGA_MESA(_DET)): elige una
/// mesa real con titular y alumnos con permiso activos, guarda un borrador por el titular
/// (nota + ausente), lo relee por Dapper, finaliza, verifica el bloqueo del titular, reabre
/// por secretaría, comprueba el estado en la home y limpia (cascada).
/// </summary>
[Trait("Category", "Integration")]
public class CargaMesaRoundtripTests
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("ESBA_TEST_CONNECTION")
        ?? "database=localhost:/pool/firebird/esba.gdb;user=sysdba;password=masterkey;charset=ISO8859_1";

    private static DbContextOptions<EsbaDbContext> Opciones =>
        new DbContextOptionsBuilder<EsbaDbContext>().UseFirebird(ConnectionString).Options;

    private static EsbaDbContext CrearContexto() => new(Opciones);

    private static FbConnectionFactory Factory() => new(ConnectionString);

    private static GuardarCargaMesaHandler Guardar(EsbaDbContext ctx) => new(
        new CargaMesaDocenteRepository(ctx), new CargaMesaDocenteQuery(Factory()),
        new GuardarCargaMesaValidator(), new EfUnitOfWork(ctx), TimeProvider.System);

    [Fact]
    public async Task Guardar_Finalizar_Reabrir_PersistenYSeLeenPorDapper()
    {
        var ct = CancellationToken.None;

        (string Carre, int Mesa, string Titular)? elegida;
        await using (var cn = await Factory().CreateOpenConnectionAsync(ct))
        {
            elegida = await cn.QuerySingleOrDefaultAsync<(string, int, string)?>("""
                SELECT FIRST 1 TRIM(M.CARRE), M.MESA, TRIM(M.TITULAR)
                FROM MESAS M
                WHERE M.TITULAR IS NOT NULL
                  AND (SELECT COUNT(*) FROM PERMEXA P JOIN ALUMNOS A ON A.COD_ALU = P.COD_ALU AND A.CARRE = P.CARRE
                       WHERE P.CARRE = M.CARRE AND P.MESA = M.MESA AND A.BAJA = 'N') >= 2
                  AND NOT EXISTS (SELECT 1 FROM DOC_CARGA_MESA DM WHERE DM.CARRE = M.CARRE AND DM.MESA = M.MESA)
                ORDER BY M.FECH_EXA DESC
                """);
        }

        Assert.NotNull(elegida);
        var (carre, mesa, titular) = elegida!.Value;
        var clave = new ClaveMesa { CodigoCarrera = carre, NumeroMesa = mesa };
        var actorTitular = new ActorCargaDocente { CodigoUsuario = 1, CodigoDocente = titular };
        var secretaria = new ActorCargaDocente { CodigoUsuario = 1, EsSecretaria = true };
        int? cargaId = null;

        try
        {
            var antes = await new CargaMesaDocenteQuery(Factory()).ObtenerAsync(clave, ct);
            Assert.NotNull(antes);
            Assert.False(antes!.TieneCarga);
            Assert.Equal(titular, antes.CodigoDocenteTitular);
            Assert.True(antes.Alumnos.Count >= 2);
            var aprobado = antes.Alumnos[0];
            var ausente = antes.Alumnos[1];

            await using (var ctx = CrearContexto())
            {
                var guardado = await Guardar(ctx).HandleAsync(new GuardarCargaMesaCommand
                {
                    Mesa = clave,
                    Actor = actorTitular,
                    Observaciones = "Roundtrip mesa",
                    Filas =
                    [
                        new FilaCargaMesaInput { CodigoAlumno = aprobado.CodigoAlumno, CodigoMateria = aprobado.CodigoMateria, Nota = 8.5m },
                        new FilaCargaMesaInput { CodigoAlumno = ausente.CodigoAlumno, CodigoMateria = ausente.CodigoMateria, Ausente = true, Observaciones = "no vino" },
                    ],
                }, ct);
                Assert.Equal(OperationStatus.Ok, guardado.Status);
                cargaId = guardado.Value;
            }

            var despues = await new CargaMesaDocenteQuery(Factory()).ObtenerAsync(clave, ct);
            Assert.Equal(cargaId, despues!.CargaId);
            Assert.Equal(EstadoCargaDocente.Borrador, despues.Estado);
            Assert.Equal("Roundtrip mesa", despues.ObservacionesCarga);
            var filaAprobado = despues.Alumnos.Single(a => a.CodigoAlumno == aprobado.CodigoAlumno && a.CodigoMateria == aprobado.CodigoMateria);
            Assert.True(filaAprobado.TieneDetalle);
            Assert.Equal(8.5m, filaAprobado.Nota);
            Assert.False(filaAprobado.Ausente);
            Assert.Equal(aprobado.PermisoIndice, filaAprobado.PermisoIndice);
            var filaAusente = despues.Alumnos.Single(a => a.CodigoAlumno == ausente.CodigoAlumno && a.CodigoMateria == ausente.CodigoMateria);
            Assert.True(filaAusente.Ausente);
            Assert.Null(filaAusente.Nota);
            Assert.Equal("no vino", filaAusente.Observaciones);

            await using (var ctx = CrearContexto())
            {
                var fin = await new FinalizarCargaMesaHandler(new CargaMesaDocenteRepository(ctx), new EfUnitOfWork(ctx), TimeProvider.System)
                    .HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = clave, Actor = actorTitular }, ct);
                Assert.Equal(OperationStatus.Ok, fin.Status);
            }

            await using (var ctx = CrearContexto())
            {
                var bloqueado = await Guardar(ctx).HandleAsync(new GuardarCargaMesaCommand { Mesa = clave, Actor = actorTitular }, ct);
                Assert.Equal(OperationStatus.Error, bloqueado.Status);
            }

            await using (var ctx = CrearContexto())
            {
                var reabierta = await new ReabrirCargaMesaHandler(new CargaMesaDocenteRepository(ctx), new EfUnitOfWork(ctx), TimeProvider.System)
                    .HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = clave, Actor = secretaria }, ct);
                Assert.Equal(OperationStatus.Ok, reabierta.Status);
            }

            var reabiertaDto = await new CargaMesaDocenteQuery(Factory()).ObtenerAsync(clave, ct);
            Assert.Equal(EstadoCargaDocente.Borrador, reabiertaDto!.Estado);

            var mesasDocente = await new AreaDocenteQuery(Factory()).ListarMesasAsync(titular, desde: null, ct);
            var enInicio = mesasDocente.Single(m => m.CodigoCarrera == carre && m.NumeroMesa == mesa);
            Assert.Equal("BOR", enInicio.EstadoCarga);
            Assert.Equal(antes.Alumnos.Count, enInicio.CantidadInscriptos);

            // Listado de secretaría + efectivizar.
            var listado = await new PrecargasDocenteQuery(Factory()).BuscarMesasAsync(
                new PrecargasDocenteFiltro { CodigoCarrera = carre, Estado = EstadoCargaDocente.Borrador }, ct);
            var enListado = Assert.Single(listado.Items, i => i.CargaId == cargaId);
            Assert.Equal(mesa, enListado.NumeroMesa);
            Assert.Equal(2, enListado.CantidadAlumnos);

            await using (var ctx = CrearContexto())
            {
                var efe = await new EfectivizarCargaMesaHandler(new CargaMesaDocenteRepository(ctx), new EfUnitOfWork(ctx), TimeProvider.System)
                    .HandleAsync(new CambiarEstadoCargaMesaCommand { Mesa = clave, Actor = secretaria }, ct);
                Assert.Equal(OperationStatus.Ok, efe.Status);
            }

            var efectivizada = await new CargaMesaDocenteQuery(Factory()).ObtenerAsync(clave, ct);
            Assert.Equal(EstadoCargaDocente.Efectivizada, efectivizada!.Estado);
        }
        finally
        {
            if (cargaId is not null)
            {
                await using var cn = await Factory().CreateOpenConnectionAsync(ct);
                await cn.ExecuteAsync("DELETE FROM DOC_CARGA_MESA WHERE ID = @Id", new { Id = cargaId });
                var detalles = await cn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM DOC_CARGA_MESA_DET WHERE CARGA_ID = @Id", new { Id = cargaId });
                Assert.Equal(0, detalles);
            }
        }
    }
}
